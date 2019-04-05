using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Decos.AspNetCore.BackgroundTasks;

using Microsoft.EntityFrameworkCore;

namespace Decos.Security.Auditing.EntityFrameworkCore
{
    /// <summary>
    /// Represents a database context where the changes to audited entities are
    /// saved automatically.
    /// </summary>
    public abstract class AuditedContext : DbContext, IAuditedContext, IAuditContext
    {
        private readonly IBackgroundTaskQueue _backgroundTaskQueue;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuditedContext"/> class
        /// with the specified dependencies.
        /// </summary>
        /// <param name="changeRecorders">
        /// A collection of objects that can record changes made in this context.
        /// </param>
        /// <param name="identity">
        /// Provides information about the currently authenticated client.
        /// </param>
        /// <param name="backgroundTaskQueue">
        /// A queue for scheduling background operations.
        /// </param>
        /// <param name="options">The options for this context.</param>
        protected AuditedContext(
            IEnumerable<IChangeRecorder> changeRecorders,
            IIdentity identity,
            IBackgroundTaskQueue backgroundTaskQueue,
            DbContextOptions options)
            : base(options)
        {
            ChangeRecorders = changeRecorders;
            Identity = identity;
            _backgroundTaskQueue = backgroundTaskQueue;
        }

        /// <summary>
        /// Gets a collection of objects that record changes to the database
        /// context.
        /// </summary>
        public IEnumerable<IChangeRecorder> ChangeRecorders { get; }

        /// <summary>
        /// Gets information about the currently authenticated client.
        /// </summary>
        public IIdentity Identity { get; }

        /// <summary>
        /// Gets or sets a collection of all change sets in the database context.
        /// </summary>
        public DbSet<ChangeSet> ChangeSets { get; set; }

        /// <summary>
        /// Gets or sets a collection of all changes in the database context.
        /// </summary>
        public DbSet<Change> Changes { get; set; }

        /// <summary>
        /// Saves all changes made in this context to the database.
        /// </summary>
        /// <param name="acceptAllChangesOnSuccess">
        /// Indicates whether <see
        /// cref="Microsoft.EntityFrameworkCore.ChangeTracking.ChangeTracker.AcceptAllChanges"/>
        /// is called after the changes have been sent successfully to the
        /// database.
        /// </param>
        /// <remarks>
        /// This method will automatically call <see
        /// cref="Microsoft.EntityFrameworkCore.ChangeTracking.ChangeTracker.DetectChanges"/>
        /// to discover any changes to entity instances before saving to the
        /// underlying database. This can be disabled via <see
        /// cref="Microsoft.EntityFrameworkCore.ChangeTracking.ChangeTracker.AutoDetectChangesEnabled"/>.
        /// </remarks>
        /// <returns>
        /// The number of state entries written to the database.
        /// </returns>
        /// <exception cref="DbUpdateException">
        /// An error is encountered while saving to the database.
        /// </exception>
        /// <exception cref="DbUpdateConcurrencyException">
        /// A concurrency violation is encountered while saving to the database.
        /// A concurrency violation occurs when an unexpected number of rows are
        /// affected during save. This is usually because the data in the
        /// database has been modified since it was loaded into memory.
        /// </exception>
        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            try
            {
                this.RecordChanges();
                var result = base.SaveChanges(acceptAllChangesOnSuccess);
                _backgroundTaskQueue.QueueBackgroundWorkItem(async shutdownToken
                    => await this.CommitRecordedChangesAsync(shutdownToken));
                return result;
            }
            catch
            {
                this.DiscardRecordedChangesAsync().GetAwaiter().GetResult();
                throw;
            }
        }

        /// <summary>
        /// Asynchronously saves all changes made in this context to the
        /// database.
        /// </summary>
        /// <param name="acceptAllChangesOnSuccess">
        /// Indicates whether <see
        /// cref="Microsoft.EntityFrameworkCore.ChangeTracking.ChangeTracker.AcceptAllChanges"/>
        /// is called after the changes have been sent successfully to the
        /// database.
        /// </param>
        /// <remarks>
        /// <para>
        /// This method will automatically call <see
        /// cref="Microsoft.EntityFrameworkCore.ChangeTracking.ChangeTracker.DetectChanges"/>
        /// to discover any changes to entity instances before saving to the
        /// underlying database. This can be disabled via <see
        /// cref="Microsoft.EntityFrameworkCore.ChangeTracking.ChangeTracker.AutoDetectChangesEnabled"/>.
        /// </para>
        /// <para>
        /// Multiple active operations on the same context instance are not
        /// supported. Use 'await' to ensure that any asynchronous operations
        /// have completed before calling another method on this context.
        /// </para>
        /// </remarks>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> to observe while waiting for the
        /// task to complete.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous save operation. The task
        /// result contains the number of state entries written to the database.
        /// </returns>
        /// <exception cref="DbUpdateException">
        /// An error is encountered while saving to the database.
        /// </exception>
        /// <exception cref="DbUpdateConcurrencyException">
        /// A concurrency violation is encountered while saving to the database.
        /// A concurrency violation occurs when an unexpected number of rows are
        /// affected during save. This is usually because the data in the
        /// database has been modified since it was loaded into memory.
        /// </exception>
        public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            try
            {
                this.RecordChanges();
                var result = await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
                _backgroundTaskQueue.QueueBackgroundWorkItem(async shutdownToken =>
                {
                    var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, shutdownToken);
                    await this.CommitRecordedChangesAsync(linkedTokenSource.Token);
                });
                return result;
            }
            catch
            {
                await this.DiscardRecordedChangesAsync(cancellationToken);
                throw;
            }
        }
    }
}