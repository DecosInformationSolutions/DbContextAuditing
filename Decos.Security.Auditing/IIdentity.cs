using System;

namespace Decos.Data.Auditing
{
    /// <summary>
    /// Defines properties that expose information about an authenticated client.
    /// </summary>
    public interface IIdentity
    {
        /// <summary>
        /// Gets a unique identifier for the currently authenticated client.
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Gets a name for the currently authenticated client.
        /// </summary>
        string Name { get; }
    }
}