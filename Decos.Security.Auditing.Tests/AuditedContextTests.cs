using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Sqlite;
using System.Threading.Tasks;

namespace Decos.Security.Auditing.Tests
{
    [TestClass]
    public class AuditedContextTests
    {
        private static IServiceProvider _serviceProvider;

        [AssemblyInitialize]
        public static void OnTestRunStarting(TestContext testContext)
        {
            _serviceProvider = new ServiceCollection()
                .AddDbContext<TestDbContext>(options =>
                {
                    options.EnableDetailedErrors();
                    options.EnableSensitiveDataLogging();
                    options.UseSqlite($"Data Source={testContext.TestName}.db");
                })
                .AddDbContextAuditing<TestDbContext, DummyIdentity>()
                .BuildServiceProvider();
        }

        [TestInitialize]
        public async Task OnTestStarting()
        {
            using (var scope = _serviceProvider.CreateScope())
            using (var context = scope.ServiceProvider.GetRequiredService<TestDbContext>())
            {
                await context.Database.EnsureCreatedAsync();
            }
        }

        [TestCleanup]
        public async Task OnTestEnded()
        {
            using (var scope = _serviceProvider.CreateScope())
            using (var context = scope.ServiceProvider.GetRequiredService<TestDbContext>())
            {
                await context.Database.EnsureDeletedAsync();
            }
        }

        [TestMethod]
        public async Task NewEntitiesShouldHaveACreatedDate()
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
    }
}
