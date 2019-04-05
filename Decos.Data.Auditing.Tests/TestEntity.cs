using System;

namespace Decos.Data.Auditing.Tests
{
    /// <summary>
    /// Represents a database entity used purely for testing purposes.
    /// </summary>
    public class TestEntity : IAuditedEntity
    {
        /// <summary>
        /// Gets or set a unique identifier of the entity.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets a generic value for testing purposes.
        /// </summary>
        public string Value1 { get; set; }

        /// <summary>
        /// Gets or sets a second generic value for testing purposes.
        /// </summary>
        public int Value2 { get; set; }

        /// <summary>
        /// Gets or sets the point in time the entity was added to the database,
        /// or <c>null</c> if the entity has not been added.
        /// </summary>
        public DateTimeOffset? Created { get; set; }

        /// <summary>
        /// Gets or sets a unique identifier of the client that added the entity
        /// to the database, or <c>null</c> if the entity has not been added.
        /// </summary>
        public string CreatedBy { get; set; }

        /// <summary>
        /// Gets or sets the point in time the entity was last changed, or
        /// <c>null</c> if the entity has not been changed since it was added.
        /// </summary>
        public DateTimeOffset? LastModified { get; set; }

        /// <summary>
        /// Gets or sets a unique identifier of the client that last changed the
        /// entity, or <c>null</c> if the entity has not been changed since it
        /// was added.
        /// </summary>
        public string LastModifiedBy { get; set; }
    }
}