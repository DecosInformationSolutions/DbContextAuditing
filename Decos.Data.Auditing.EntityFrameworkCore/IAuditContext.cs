using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

namespace Decos.Data.Auditing.EntityFrameworkCore
{
    /// <summary>
    /// Defines a database context that can be used to store changes to audited
    /// entities.
    /// </summary>
    public interface IAuditContext
    {
        /// <summary>
        /// Gets or sets a collection of all change sets in the database context.
        /// </summary>
        DbSet<ChangeSet> ChangeSets { get; set; }

        /// <summary>
        /// Gets or sets a collection of all changes in the database context.
        /// </summary>
        DbSet<Change> Changes { get; set; }

        /// <summary>
        /// Asynchronously saves all changes made in this context to the
        /// database.
        /// </summary>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> to observe while waiting for the
        /// task to complete.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous save operation. The task
        /// result contains the number of state entries written to the database.
        /// </returns>
        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
}