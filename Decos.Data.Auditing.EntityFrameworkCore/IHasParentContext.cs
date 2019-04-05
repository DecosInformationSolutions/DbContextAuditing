using System;

namespace Decos.Data.Auditing.EntityFrameworkCore
{
    /// <summary>
    /// Defines a property for passing the active database context to internal
    /// auditing services.
    /// </summary>
    public interface IHasParentContext
    {
        /// <summary>
        /// Gets or sets a reference to the parent context.
        /// </summary>
        IAuditContext ParentContext { get; set; }
    }
}