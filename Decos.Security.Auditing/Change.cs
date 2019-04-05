using System;

namespace Decos.Data.Auditing
{
    /// <summary>
    /// Represents a change to a property.
    /// </summary>
    public class Change
    {
        /// <summary>
        /// Gets or set a unique identifier for the change.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets a unique identifier for the change set the change is
        /// part of.
        /// </summary>
        public long ChangeSetId { get; set; }

        /// <summary>
        /// Gets or sets the change set the change is part of.
        /// </summary>
        public virtual ChangeSet ChangeSet { get; set; }

        /// <summary>
        /// Gets or sets a unique identifier of the entity that changed.
        /// </summary>
        public Guid EntityId { get; set; }

        /// <summary>
        /// Gets or sets the type of entity that changed.
        /// </summary>
        public Type EntityType { get; set; }

        /// <summary>
        /// Gets or sets the name of the property whose value changed.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the type of the property whose value changed.
        /// </summary>
        public Type Type { get; set; }

        /// <summary>
        /// Gets or sets a JSON-serialized string that contains the value of the
        /// property before the change.
        /// </summary>
        public string PreviousValue { get; set; }

        /// <summary>
        /// Gets or sets a JSON-serialized string that contains the value of the
        /// property after the change.
        /// </summary>
        public string NewValue { get; set; }
    }
}