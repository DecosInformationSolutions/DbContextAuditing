using System;
using System.Collections.Generic;

namespace Decos.Security.Auditing
{
    /// <summary>
    /// Represents a transaction of one or more changes to audited database
    /// entities.
    /// </summary>
    public class ChangeSet
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ChangeSet"/> class.
        /// </summary>
        public ChangeSet()
        {
        }

        /// <summary>
        /// Gets or sets a unique identifier of the change set.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Gets or sets the point in time the change set was created.
        /// </summary>
        public DateTimeOffset Created { get; set; }

        /// <summary>
        /// Gets or sets a unique identifier of the client that caused the
        /// changes in the change set.
        /// </summary>
        public string CreatedBy { get; set; }

        /// <summary>
        /// Gets or sets a collection of the changes in the change set.
        /// </summary>
        public virtual ICollection<Change> Changes { get; }
            = new HashSet<Change>();

        /// <summary>
        /// Returns a string that represents the change set.
        /// </summary>
        /// <returns>A string that represents this change set.</returns>
        public override string ToString()
            => $"{Id}";
    }
}