using System;

using Newtonsoft.Json;

namespace Decos.Security.Auditing.EntityFrameworkCore
{
    /// <summary>
    /// Records changes and stores them in the same database context.
    /// </summary>
    public class SameContextChangeRecorder : ChangeRecorder, IHasParentContext
    {
        private readonly IIdentity _identity;

        /// <summary>
        /// Initializes a new instance of the <see
        /// cref="SameContextChangeRecorder"/> class for the specified context.
        /// </summary>
        /// <param name="identity">
        /// Information about the currently authenticated client.
        /// </param>
        public SameContextChangeRecorder(IIdentity identity)
        {
            _identity = identity;
        }

        /// <summary>
        /// Gets or sets the context where recorded changes are stored.
        /// </summary>
        public IAuditContext ParentContext { get; set; }

        /// <summary>
        /// Processes recorded changes before they are committed to the
        /// underlying database.
        /// </summary>
        public override void OnSavingChanges()
        {
            if (ParentContext == null)
                throw new InvalidOperationException($"The {nameof(ParentContext)} property has not been set.");

            var changeSet = new ChangeSet
            {
                Created = DateTimeOffset.Now,
                CreatedBy = _identity.Id
            };

            foreach (var change in Modifications)
                changeSet.Changes.Add(CreateChangeRecord(change));

            if (changeSet.Changes.Count > 0)
                ParentContext.ChangeSets.Add(changeSet);
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