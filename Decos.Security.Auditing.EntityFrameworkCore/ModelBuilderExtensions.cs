using System;

using Microsoft.EntityFrameworkCore;

namespace Decos.Security.Auditing.EntityFrameworkCore
{
    /// <summary>
    /// Provides a set of static methods for configuring a model for auditing.
    /// </summary>
    public static class ModelBuilderExtensions
    {
        /// <summary>
        /// Configures auditing in the model.
        /// </summary>
        /// <param name="modelBuilder">The model builder to configure.</param>
        /// <returns>The configured model builder.</returns>
        public static ModelBuilder UseAuditing(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ChangeSet>()
                .Property(x => x.Id)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<ChangeSet>()
                .HasMany(x => x.Changes)
                .WithOne(x => x.ChangeSet)
                .IsRequired();

            modelBuilder.Entity<Change>()
                .Property(x => x.Id)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<Change>()
                .Property(x => x.EntityId)
                .IsRequired();

            modelBuilder.Entity<Change>()
                .Property(x => x.Name)
                .IsRequired();

            modelBuilder.Entity<Change>()
                .Property(x => x.Type)
                .IsRequired()
                .HasConversion(
                    convertToProviderExpression: x => x.AssemblyQualifiedName,
                    convertFromProviderExpression: x => Type.GetType(x));

            modelBuilder.Entity<Change>()
                .HasIndex(x => x.EntityId);

            return modelBuilder;
        }
    }
}