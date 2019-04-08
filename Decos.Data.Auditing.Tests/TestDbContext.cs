using System;
using System.Collections.Generic;

using Decos.AspNetCore.BackgroundTasks;
using Decos.Data.Auditing.EntityFrameworkCore;

using Microsoft.EntityFrameworkCore;

namespace Decos.Data.Auditing.Tests
{
    public class TestDbContext : AuditedContext
    {
        public TestDbContext(DbContextOptions options)
            : base(options)
        {
        }

        public TestDbContext(IIdentity identity, DbContextOptions options)
            : base(identity, options)
        {
        }

        public TestDbContext(
            IEnumerable<IChangeRecorder> changeRecorders,
            IIdentity identity,
            IBackgroundTaskQueue backgroundTaskQueue,
            DbContextOptions options)
            : base(changeRecorders, identity, backgroundTaskQueue, options)
        {
        }

        public virtual DbSet<TestEntity> TestEntities { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.UseAuditing();
        }
    }
}