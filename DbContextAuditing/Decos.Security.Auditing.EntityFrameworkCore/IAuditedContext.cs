using System;
using System.Collections.Generic;
using Decos.AspNetCore.BackgroundTasks;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Decos.Security.Auditing.EntityFrameworkCore
{
    /// <summary>
    /// Defines a database context of which the changes to entities can be
    /// recorded.
    /// </summary>
    public interface IAuditedContext
    {
        /// <summary>
        /// Gets a collection of objects that record changes to the database
        /// context.
        /// </summary>
        IEnumerable<IChangeRecorder> ChangeRecorders { get; }

        /// <summary>
        /// Provides access to information and operations for entity instances
        /// this context is tracking.
        /// </summary>
        ChangeTracker ChangeTracker { get; }

        /// <summary>
        /// Gets information about the currently authenticated client.
        /// </summary>
        IIdentity Identity { get; }
    }
}