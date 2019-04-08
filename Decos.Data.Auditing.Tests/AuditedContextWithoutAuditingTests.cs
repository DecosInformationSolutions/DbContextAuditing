using System;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Decos.Data.Auditing.Tests
{
    [TestClass]
    public class AuditedContextWithoutAuditingTests
    {
        private static IServiceProvider _serviceProvider;

        [ClassInitialize]
        public static void OnTestsStarting(TestContext testContext)
        {
            _serviceProvider = new ServiceCollection()
                .AddDbContext<TestDbContext>(options =>
                {
                    options.EnableDetailedErrors();
                    options.EnableSensitiveDataLogging();
                    options.UseSqlite($"Data Source={testContext.TestName}.db");
                })
                .BuildServiceProvider();
        }

        [TestInitialize]
        public async Task OnTestStarting()
        {
            using (var scope = _serviceProvider.CreateScope())
            using (var context = scope.ServiceProvider.GetRequiredService<TestDbContext>())
                await context.Database.EnsureCreatedAsync();
        }

        [TestCleanup]
        public async Task OnTestEnded()
        {
            using (var scope = _serviceProvider.CreateScope())
            using (var context = scope.ServiceProvider.GetRequiredService<TestDbContext>())
                await context.Database.EnsureDeletedAsync();
        }

        [TestMethod]
        public async Task WithoutAuditingServicesNewEntitiesShouldStillHaveACreatedDate()
        {
            using (var scope = _serviceProvider.CreateScope())
            using (var context = scope.ServiceProvider.GetRequiredService<TestDbContext>())
            {
                context.TestEntities.Add(new TestEntity());
                await context.SaveChangesAsync();
            }

            using (var scope = _serviceProvider.CreateScope())
            using (var context = scope.ServiceProvider.GetRequiredService<TestDbContext>())
            {
                var entity = await context.TestEntities.SingleAsync();
                Assert.IsNotNull(entity.Created);
            }
        }

        [TestMethod]
        public async Task WithoutAuditingChangingAValueShouldStillUpdateModifiedDate()
        {
            Guid id;
            using (var scope = _serviceProvider.CreateScope())
            using (var context = scope.ServiceProvider.GetRequiredService<TestDbContext>())
            {
                var entity = new TestEntity();
                context.TestEntities.Add(entity);
                await context.SaveChangesAsync();

                id = entity.Id;
                entity.Value1 = "Test";
                await context.SaveChangesAsync();
            }

            using (var scope = _serviceProvider.CreateScope())
            using (var context = scope.ServiceProvider.GetRequiredService<TestDbContext>())
            {
                var entity = await context.TestEntities.SingleAsync();
                Assert.IsNotNull(entity.LastModified);
            }
        }

        [TestMethod]
        public async Task WithoutAuditingChangingAValueShouldNotAddAChangeRecord()
        {
            Guid id;
            using (var scope = _serviceProvider.CreateScope())
            using (var context = scope.ServiceProvider.GetRequiredService<TestDbContext>())
            {
                var entity = new TestEntity();
                context.TestEntities.Add(entity);
                await context.SaveChangesAsync();

                id = entity.Id;
                entity.Value1 = "Test";
                await context.SaveChangesAsync();
            }

            using (var scope = _serviceProvider.CreateScope())
            using (var context = scope.ServiceProvider.GetRequiredService<TestDbContext>())
            {
                Assert.IsFalse(await context.Changes.AnyAsync());
            }
        }

        [TestMethod]
        public async Task WithoutAuditingChangingAValueShouldNotAddAChangeRecord_Synchronous()
        {
            Guid id;
            using (var scope = _serviceProvider.CreateScope())
            using (var context = scope.ServiceProvider.GetRequiredService<TestDbContext>())
            {
                var entity = new TestEntity();
                context.TestEntities.Add(entity);
                context.SaveChanges();

                id = entity.Id;
                entity.Value1 = "Test";
                context.SaveChanges();
            }

            using (var scope = _serviceProvider.CreateScope())
            using (var context = scope.ServiceProvider.GetRequiredService<TestDbContext>())
            {
                Assert.IsFalse(await context.Changes.AnyAsync());
            }
        }
    }
}