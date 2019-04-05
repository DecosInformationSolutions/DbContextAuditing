using System;
using System.Threading;
using System.Threading.Tasks;

namespace Decos.Data.Auditing
{
    /// <summary>
    /// Defines methods to record changes to entities in the database.
    /// </summary>
    public interface IChangeRecorder
    {
        /// <summary>
        /// Occurs before changes are saved to the underlying database.
        /// </summary>
        void OnSavingChanges();

        /// <summary>
        /// Occurs when the recorded changes have been saved successfully.
        /// </summary>
        /// <returns>
        /// A task representing the result of the asynchronous operation.
        /// </returns>
        Task CommitAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Occurs when the recorded changes have been discarded, such as due to
        /// an exception when saving the changes.
        /// </summary>
        /// <returns>
        /// A task representing the result of the asynchronous operation.
        /// </returns>
        Task DiscardAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Records the change that occurs when a new entity is added.
        /// </summary>
        /// <param name="entity">The entity that was added.</param>
        void RecordAdd(IAuditedEntity entity);

        /// <summary>
        /// Records the change that occurs when a property of an entity is
        /// modified.
        /// </summary>
        /// <param name="entity">The entity that was modified.</param>
        /// <param name="propertyName">
        /// The name of the property that was modified.
        /// </param>
        /// <param name="originalValue">
        /// The value of the property before it was modified.
        /// </param>
        /// <param name="currentValue">The current value of the property.</param>
        void RecordChange(IAuditedEntity entity, string propertyName, object originalValue, object currentValue);

        /// <summary>
        /// Records the change that occurs when an entity is deleted.
        /// </summary>
        /// <param name="entity">The entity that was deleted.</param>
        void RecordDelete(IAuditedEntity entity);
    }
}