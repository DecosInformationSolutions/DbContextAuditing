using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Decos.Security.Auditing
{
    /// <summary>
    /// Provides a base for classes that record changes to entities in the
    /// database.
    /// </summary>
    public abstract class ChangeRecorder : IChangeRecorder
    {
        private readonly List<IAuditedEntity> additions;
        private readonly List<IAuditedEntity> deletions;
        private readonly List<EntityChange> modifications;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChangeRecorder"/> class.
        /// </summary>
        protected ChangeRecorder()
        {
            additions = new List<IAuditedEntity>();
            deletions = new List<IAuditedEntity>();
            modifications = new List<EntityChange>();
        }

        /// <summary>
        /// Gets a collection containing the added entities.
        /// </summary>
        protected IReadOnlyCollection<IAuditedEntity> Additions
            => additions.AsReadOnly();

        /// <summary>
        /// Gets a collection containing the deleted entities.
        /// </summary>
        protected IReadOnlyCollection<IAuditedEntity> Deletions
            => deletions.AsReadOnly();

        /// <summary>
        /// Gets a collection containing the recorded modifications.
        /// </summary>
        protected IReadOnlyCollection<EntityChange> Modifications
            => modifications.AsReadOnly();

        /// <summary>
        /// When overridden in a derived class, processes recorded changes before
        /// they are committed to the underlying database.
        /// </summary>
        public virtual void OnSavingChanges()
        {
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
        public virtual Task CommitAsync(CancellationToken cancellationToken)
        {
            additions.Clear();
            deletions.Clear();
            modifications.Clear();
            return Task.CompletedTask;
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
        public virtual Task DiscardAsync(CancellationToken cancellationToken)
        {
            additions.Clear();
            deletions.Clear();
            modifications.Clear();
            return Task.CompletedTask;
        }

        /// <summary>
        /// Records the change that occurs when a new entity is added.
        /// </summary>
        /// <param name="entity">The entity that was added.</param>
        public virtual void RecordAdd(IAuditedEntity entity)
        {
            additions.Add(entity);
        }

        /// <summary>
        /// Records the change that occurs when an entity is deleted.
        /// </summary>
        /// <param name="entity">The entity that was deleted.</param>
        public virtual void RecordDelete(IAuditedEntity entity)
        {
            deletions.Add(entity);
        }

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
        public virtual void RecordChange(
            IAuditedEntity entity,
            string propertyName,
            object originalValue,
            object currentValue)
        {
            modifications.Add(new EntityChange(entity, propertyName, originalValue, currentValue));
        }

        /// <summary>
        /// Represents an entity that has been modified.
        /// </summary>
        protected class EntityChange
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="EntityChange"/>
            /// class for the specified entity.
            /// </summary>
            /// <param name="entity">The entity that was modified.</param>
            /// <param name="propertyName">
            /// The name of the property that was modified.
            /// </param>
            /// <param name="originalValue">
            /// The value of the property before it was modified.
            /// </param>
            /// <param name="currentValue">
            /// The current value of the property.
            /// </param>
            public EntityChange(
                IAuditedEntity entity,
                string propertyName,
                object originalValue,
                object currentValue)
            {
                if (entity == null)
                    throw new ArgumentNullException(nameof(entity));

                Entity = entity;
                PropertyName = propertyName;
                OriginalValue = originalValue;
                CurrentValue = currentValue;
            }

            /// <summary>
            /// Gets the entity that was modified.
            /// </summary>
            public IAuditedEntity Entity { get; }

            /// <summary>
            /// Gets the type of the entity that was modified.
            /// </summary>
            public Type EntityType => Entity.GetType();

            /// <summary>
            /// Gets the name of the property that was modified.
            /// </summary>
            public string PropertyName { get; }

            /// <summary>
            /// Get the type of the property that was modified.
            /// </summary>
            public Type PropertyType => GetPropertyType(EntityType, PropertyName);

            /// <summary>
            /// Gets the value of the property before it was modified.
            /// </summary>
            public object OriginalValue { get; }

            /// <summary>
            /// Gets the current value of the property.
            /// </summary>
            public object CurrentValue { get; }

            private static Type GetPropertyType(Type entityType, string propertyName)
            {
                var property = entityType.GetProperty(propertyName);
                return property?.PropertyType;
            }
        }
    }
}