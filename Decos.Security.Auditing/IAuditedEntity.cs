using System;

namespace Decos.Data.Auditing
{
    /// <summary>
    /// Defines a database entity that can be audited.
    /// </summary>
    public interface IAuditedEntity
    {
        /// <summary>
        /// Gets or set a unique identifier of the entity.
        /// </summary>
        Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the point in time the entity was added to the database,
        /// or <c>null</c> if the entity has not been added.
        /// </summary>
        DateTimeOffset? Created { get; set; }

        /// <summary>
        /// Gets or sets a unique identifier of the client that added the entity
        /// to the database, or <c>null</c> if the entity has not been added.
        /// </summary>
        string CreatedBy { get; set; }

        /// <summary>
        /// Gets or sets the point in time the entity was last changed, or
        /// <c>null</c> if the entity has not been changed since it was added.
        /// </summary>
        DateTimeOffset? LastModified { get; set; }

        /// <summary>
        /// Gets or sets a unique identifier of the client that last changed the
        /// entity, or <c>null</c> if the entity has not been changed since it
        /// was added.
        /// </summary>
        string LastModifiedBy { get; set; }
    }
}