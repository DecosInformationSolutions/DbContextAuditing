using System;
using System.Threading;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace Decos.Security.Auditing.EntityFrameworkCore
{
    /// <summary>
    /// Records changes and stores them in a database context.
    /// </summary>
    public class AuditContextChangeRecorder : ChangeRecorder
    {
        private readonly IIdentity _identity;

        /// <summary>
        /// Initializes a new instance of the <see
        /// cref="AuditContextChangeRecorder"/> class for the specified context.
        /// </summary>
        /// <param name="context">
        /// The database context used to store the recorded changes.
        /// </param>
        /// <param name="identity">
        /// Information about the currently authenticated client.
        /// </param>
        public AuditContextChangeRecorder(IAuditContext context, IIdentity identity)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            Context = context;
            _identity = identity;
        }

        /// <summary>
        /// Gets the context where recorded changes are stored.
        /// </summary>
        protected IAuditContext Context { get; }

        /// <summary>
        /// Gets a change set that contains the recorded changes, or <c>null</c>
        /// if no save is currently in progress.
        /// </summary>
        protected ChangeSet ChangeSet { get; set; }

        /// <summary>
        /// Processes recorded changes before they are committed to the
        /// underlying database.
        /// </summary>
        public override void OnSavingChanges()
        {
            ChangeSet = new ChangeSet
            {
                Created = DateTimeOffset.Now,
                CreatedBy = _identity.Id
            };

            foreach (var change in Modifications)
                ChangeSet.Changes.Add(CreateChangeRecord(change));
        }

        /// <summary>
        /// Commits changes that have been recorded.
        /// </summary>
        /// <param name="cancellationToken">
        /// A token to monitor for cancellation requests.
        /// </param>
        /// <returns>
        /// A task representing the result of the asynchronous operation.
        /// </returns>
        public override async Task CommitAsync(CancellationToken cancellationToken)
        {
            try
            {
                Context.ChangeSets.Add(ChangeSet);
                await Context.SaveChangesAsync(cancellationToken);
            }
            finally
            {
                await base.CommitAsync(cancellationToken);
                ChangeSet = null;
            }
        }

        /// <summary>
        /// Discards changes that have been recorded.
        /// </summary>
        /// <param name="cancellationToken">
        /// A token to monitor for cancellation requests.
        /// </param>
        /// <returns>
        /// A task representing the result of the asynchronous operation.
        /// </returns>
        public override async Task DiscardAsync(CancellationToken cancellationToken)
        {
            await base.DiscardAsync(cancellationToken);
            ChangeSet = null;
        }

        /// <summary>
        /// Creates a change record for the specified entity change.
        /// </summary>
        /// <param name="change">The recorded change.</param>
        /// <returns>
        /// A new <see cref="Change"/> for <paramref name="change"/>.
        /// </returns>
        protected virtual Change CreateChangeRecord(EntityChange change)
        {
            var previousValue = JsonConvert.SerializeObject(change.OriginalValue);
            var newValue = JsonConvert.SerializeObject(change.CurrentValue);

            return new Change
            {
                EntityId = change.Entity.Id,
                EntityType = change.EntityType,
                Name = change.PropertyName,
                Type = change.PropertyType,
                PreviousValue = previousValue,
                NewValue = newValue
            };
        }
    }
}