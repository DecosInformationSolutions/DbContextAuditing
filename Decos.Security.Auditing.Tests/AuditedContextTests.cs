using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Sqlite;
using System.Threading.Tasks;
using Decos.AspNetCore.BackgroundTasks;
using System.Threading;
using System.Linq;

namespace Decos.Security.Auditing.Tests
{
    [TestClass]
    public class AuditedContextTests
    {
        private static IServiceProvider _serviceProvider;
        private IBackgroundTaskQueue _backgroundTaskQueue;

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

        [TestMethod]
        public async Task UpdatingAnExistingEntityAddsAChangeRecord()
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
                var change = await context.Changes.SingleOrDefaultAsync(x => x.EntityId == id);
                Assert.AreEqual(nameof(TestEntity.Value1), change.Name);
            }
        }

        private async Task RunAllBackgroundTasksAsync(CancellationToken cancellationToken = default)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var queue = scope.ServiceProvider.GetRequiredService<IBackgroundTaskQueue>();

                var order = await queue.DequeueAsync(cancellationToken);
                while (order != null)
                {
                    var workerType = order
                        .GetType()
                        .GetInterfaces()
                        .First(t => t.IsConstructedGenericType && t.GetGenericTypeDefinition() == typeof(IBackgroundWorkOrder<,>))
                        .GetGenericArguments()
                        .Last();

                    var worker = scope.ServiceProvider
                        .GetRequiredService(workerType);

                    var task = (Task)workerType
                        .GetMethod("DoWorkAsync")
                        .Invoke(worker, new object[] { order, cancellationToken });
                    await task;

                    order = await queue.DequeueAsync(cancellationToken);
                }
            }
        }
    }
}
