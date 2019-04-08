using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Decos.Data.Auditing.EntityFrameworkCore
{
    /// <summary>
    /// Provides a set of static methods for managing changes in an audited
    /// database context.
    /// </summary>
    public static class AuditedContextExtensions
    {
        /// <summary>
        /// Reverts pending changes made to the database context.
        /// </summary>
        /// <param name="context">
        /// The database context whose pending changes to revert.
        /// </param>
        /// <param name="cancellationToken">
        /// A token to monitor for cancellation requests.
        /// </param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public static async Task UndoChangesAsync(this IAuditedContext context, CancellationToken cancellationToken = default)
        {
            if (context.ChangeTracker.AutoDetectChangesEnabled)
                context.ChangeTracker.DetectChanges();

            foreach (var entry in context.ChangeTracker.Entries())
                switch (entry.State)
                {
                    case EntityState.Deleted:
                        entry.State = EntityState.Unchanged;
                        break;

                    case EntityState.Modified:
                        await entry.ReloadAsync(cancellationToken);
                        break;

                    case EntityState.Added:
                        entry.State = EntityState.Detached;
                        break;
                }
        }

        /// <summary>
        /// Records the currently tracked changes in the database context to all
        /// change recorders.
        /// </summary>
        /// <param name="context">
        /// The database context that contains the changes.
        /// </param>
        public static void RecordChanges(this IAuditedContext context)
        {
            if (context.ChangeTracker.AutoDetectChangesEnabled)
                context.ChangeTracker.DetectChanges();

            foreach (var entity in context.ChangeTracker.Entries().Where(e => e.Entity is IAuditedEntity))
            {
                var auditedEntity = (IAuditedEntity)entity.Entity;
                switch (entity.State)
                {
                    case EntityState.Added:
                        auditedEntity.Created = DateTimeOffset.Now;
                        auditedEntity.CreatedBy = context.Identity?.Id;
                        break;

                    case EntityState.Modified:
                        auditedEntity.LastModified = DateTimeOffset.Now;
                        auditedEntity.LastModifiedBy = context.Identity?.Id;
                        break;
                }

                context.RecordChange(entity, auditedEntity);
            }

            if (context.CanAudit())
            {
                foreach (var changeRecorder in context.ChangeRecorders)
                {
                    if (changeRecorder is IHasParentContext parentContextRecorder)
                        parentContextRecorder.ParentContext = (IAuditContext)context;

                    changeRecorder.OnSavingChanges();
                }
            }
        }

        /// <summary>
        /// Saves the prepared audit records of the saved changes. This method
        /// should be called after the actual changes have successfully been
        /// committed to the database.
        /// </summary>
        /// <param name="context">
        /// The database context whose changes were recorded.
        /// </param>
        /// <param name="cancellationToken">
        /// A token to monitor for cancellation requests.
        /// </param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public static async Task CommitRecordedChangesAsync(this IAuditedContext context, CancellationToken cancellationToken = default)
        {
            if (!context.CanAudit())
                return;

            await Task.WhenAll(context.ChangeRecorders.Select(x => x.CommitAsync(cancellationToken)));
        }

        /// <summary>
        /// Discards the prepared audit records. This method should be called
        /// when the actual changes could not be saved to the database.
        /// </summary>
        /// <param name="context">
        /// The database context whose changes were recorded.
        /// </param>
        /// <param name="cancellationToken">
        /// A token to monitor for cancellation requests.
        /// </param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public static async Task DiscardRecordedChangesAsync(this IAuditedContext context, CancellationToken cancellationToken = default)
        {
            if (!context.CanAudit())
                return;

            await Task.WhenAll(context.ChangeRecorders.Select(x => x.DiscardAsync(cancellationToken)));
        }

        /// <summary>
        /// Determines whether the database context is configured to support
        /// auditing.
        /// </summary>
        /// <param name="auditedContext">The database context.</param>
        /// <returns>
        /// <c>true</c> if the services required for auditing are set; otherwise,
        /// <c>false</c>.
        /// </returns>
        public static bool CanAudit(this IAuditedContext auditedContext)
        {
            if (auditedContext == null)
                throw new ArgumentNullException(nameof(auditedContext));

            return auditedContext.ChangeRecorders?.Any() == true
                && auditedContext.Identity != null;
        }

        private static void RecordChange(this IAuditedContext context, EntityEntry entity, IAuditedEntity auditedEntity)
        {
            if (!context.CanAudit())
                return;

            switch (entity.State)
            {
                case EntityState.Added:
                    foreach (var changeRecorder in context.ChangeRecorders)
                        changeRecorder.RecordAdd(auditedEntity);
                    break;

                case EntityState.Deleted:
                    foreach (var changeRecorder in context.ChangeRecorders)
                        changeRecorder.RecordDelete(auditedEntity);
                    break;

                case EntityState.Modified:
                    foreach (var property in entity.CurrentValues.Properties)
                    {
                        var entry = entity.Property(property.Name);
                        // Note: do not change the line below into `!=`!
                        // `Equals(object, object)` calls an overridden Equals,
                        // whereas `object != object` does not.
                        if (entry.IsModified && !Equals(entry.CurrentValue, entry.OriginalValue))
                            foreach (var changeRecorder in context.ChangeRecorders)
                                changeRecorder.RecordChange(auditedEntity, property.Name, entry.OriginalValue, entry.CurrentValue);
                    }
                    break;
            }
        }
    }
}