using System;
using Decos.Security.Auditing;
using Decos.Security.Auditing.EntityFrameworkCore;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Provides a set of static methods for registering database context
    /// auditing services.
    /// </summary>
    public static class AuditedContextServiceCollectionExtensions
    {
        /// <summary>
        /// Adds the services required for database context auditing. Note that
        /// an implementation of <see cref="IIdentity"/> needs to be registered
        /// with the service collection as well.
        /// </summary>
        /// <typeparam name="TContext">
        /// The type of database context that is used to store audit records.
        /// This can be the same as or different from the context that is being
        /// audited.
        /// </typeparam>
        /// <param name="services">
        /// The service collection to add the services to.
        /// </param>
        /// <returns>A reference to the same service collection.</returns>
        public static IServiceCollection AddDbContextAuditing<TContext>(this IServiceCollection services)
            where TContext : class, IAuditContext, IAuditedContext
        {
            return services
                .AddScoped<IChangeRecorder, SameContextChangeRecorder>()
                .AddBackgroundTasks()
                .AddScoped<IAuditContext, TContext>();
        }

        /// <summary>
        /// Adds the services required for database context auditing.
        /// </summary>
        /// <typeparam name="TContext">The type of database context.</typeparam>
        /// <typeparam name="TIdentity">
        /// The type of class used to provide information about the currently
        /// authenticated client.
        /// </typeparam>
        /// <param name="services">
        /// The service collection to add the services to.
        /// </param>
        /// <returns>A reference to the same service collection.</returns>
        public static IServiceCollection AddDbContextAuditing<TContext, TIdentity>(this IServiceCollection services)
            where TContext : class, IAuditContext, IAuditedContext
            where TIdentity : class, IIdentity
        {
            return services
                .AddDbContextAuditing<TContext>()
                .AddScoped<IIdentity, TIdentity>();
        }
    }
}