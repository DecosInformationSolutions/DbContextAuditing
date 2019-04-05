using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Decos.AspNetCore.BackgroundTasks;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Decos.Data.Auditing.Tests
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
        public async Task ChangingAValueUpdatesModifiedDate()
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
                await RunAllBackgroundTasksAsync();
            }

            using (var scope = _serviceProvider.CreateScope())
            using (var context = scope.ServiceProvider.GetRequiredService<TestDbContext>())
            {
                var entity = await context.TestEntities.SingleAsync();
                Assert.IsNotNull(entity.LastModified);
            }
        }

        [TestMethod]
        public async Task ChangingAValueAddsAChangeRecord()
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
                await RunAllBackgroundTasksAsync();
            }

            using (var scope = _serviceProvider.CreateScope())
            using (var context = scope.ServiceProvider.GetRequiredService<TestDbContext>())
            {
                var change = await context.Changes.SingleOrDefaultAsync(x => x.EntityId == id);
                Assert.AreEqual(nameof(TestEntity.Value1), change.Name);
            }
        }

        [TestMethod]
        public async Task ChangingAValueAddsAChangeRecord_Synchronous()
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
                await RunAllBackgroundTasksAsync();
            }

            using (var scope = _serviceProvider.CreateScope())
            using (var context = scope.ServiceProvider.GetRequiredService<TestDbContext>())
            {
                var change = await context.Changes.SingleOrDefaultAsync(x => x.EntityId == id);
                Assert.AreEqual(nameof(TestEntity.Value1), change.Name);
            }
        }

        [TestMethod]
        public async Task ChangingMultipleValuesAtOnceCreatesASingleChangeSetWithMultipleChanges()
        {
            using (var scope = _serviceProvider.CreateScope())
            using (var context = scope.ServiceProvider.GetRequiredService<TestDbContext>())
            {
                var entity = new TestEntity();
                context.TestEntities.Add(entity);
                await context.SaveChangesAsync();

                entity.Value1 = "Test";
                entity.Value2 = 2;
                await context.SaveChangesAsync();
                await RunAllBackgroundTasksAsync();
            }

            using (var scope = _serviceProvider.CreateScope())
            using (var context = scope.ServiceProvider.GetRequiredService<TestDbContext>())
            {
                var changeSet = await context.ChangeSets.SingleAsync();
                var changes = await context.Changes.Where(x => x.ChangeSetId == changeSet.Id).ToListAsync();
                Assert.AreEqual(2, changes.Count);
            }
        }

        [TestMethod]
        public async Task ChangingMultipleValuesMultipleTimesCreatesAMultipleChangeSets()
        {
            using (var scope = _serviceProvider.CreateScope())
            using (var context = scope.ServiceProvider.GetRequiredService<TestDbContext>())
            {
                var entity = new TestEntity();
                context.TestEntities.Add(entity);
                await context.SaveChangesAsync();

                entity.Value1 = "Test";
                await context.SaveChangesAsync();
                await RunAllBackgroundTasksAsync();

                entity.Value2 = 2;
                await context.SaveChangesAsync();
                await RunAllBackgroundTasksAsync();
            }

            using (var scope = _serviceProvider.CreateScope())
            using (var context = scope.ServiceProvider.GetRequiredService<TestDbContext>())
            {
                var change1 = await context.Changes.SingleAsync(x => x.Name == nameof(TestEntity.Value1));
                var change2 = await context.Changes.SingleAsync(x => x.Name == nameof(TestEntity.Value2));
                Assert.AreNotEqual(change1.ChangeSetId, change2.ChangeSetId);
            }
        }

        [TestMethod]
        public async Task ChangingMultipleEntitiesAtOnceCreatesASingleChangeSetWithMultipleChanges()
        {
            Guid id1;
            Guid id2;
            using (var scope = _serviceProvider.CreateScope())
            using (var context = scope.ServiceProvider.GetRequiredService<TestDbContext>())
            {
                var entity1 = new TestEntity();
                context.TestEntities.Add(entity1);
                var entity2 = new TestEntity();
                context.TestEntities.Add(entity2);
                await context.SaveChangesAsync();
                id1 = entity1.Id;
                id2 = entity2.Id;

                entity1.Value2 = 1;
                entity2.Value2 = 2;
                await context.SaveChangesAsync();
                await RunAllBackgroundTasksAsync();
            }

            using (var scope = _serviceProvider.CreateScope())
            using (var context = scope.ServiceProvider.GetRequiredService<TestDbContext>())
            {
                var change1 = await context.Changes.SingleAsync(x => x.EntityId == id1);
                var change2 = await context.Changes.SingleAsync(x => x.EntityId == id2);
                Assert.AreEqual(change1.ChangeSetId, change2.ChangeSetId);
            }
        }

        [TestMethod]
        public async Task ChangedValueCanBeDeserialized()
        {
            const string testValue = "Te\n💩st";
            using (var scope = _serviceProvider.CreateScope())
            using (var context = scope.ServiceProvider.GetRequiredService<TestDbContext>())
            {
                var entity = new TestEntity();
                context.TestEntities.Add(entity);
                context.SaveChanges();

                entity.Value1 = testValue;
                context.SaveChanges();
                await RunAllBackgroundTasksAsync();
            }

            using (var scope = _serviceProvider.CreateScope())
            using (var context = scope.ServiceProvider.GetRequiredService<TestDbContext>())
            {
                var change = await context.Changes.SingleAsync();
                var value = Newtonsoft.Json.JsonConvert.DeserializeObject(change.NewValue, change.Type);
                Assert.AreEqual(testValue, value);
            }
        }

        private async Task RunAllBackgroundTasksAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var queue = scope.ServiceProvider.GetRequiredService<IBackgroundTaskQueue>();

                    var dequeueTimeout = CancellationTokenSource.CreateLinkedTokenSource(
                        cancellationToken,
                        new CancellationTokenSource(10).Token);
                    var order = await queue.DequeueAsync(dequeueTimeout.Token);
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

                        dequeueTimeout = CancellationTokenSource.CreateLinkedTokenSource(
                            cancellationToken,
                            new CancellationTokenSource(10).Token);
                        order = await queue.DequeueAsync(dequeueTimeout.Token);
                    }
                }
            }
            catch (OperationCanceledException) { }
        }
    }
}