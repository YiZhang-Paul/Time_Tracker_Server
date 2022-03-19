using Core.DbContexts;
using Core.Enums;
using Core.Models.Event;
using NUnit.Framework;
using Service.Repositories;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Service.Test.Integration.Repositories
{
    [TestFixture]
    [Category("Integration")]
    public class EventHistoryRepositoryTest
    {
        private TimeTrackerDbContext Context { get; set; }
        private EventHistoryRepository Subject { get; set; }

        [SetUp]
        public async Task Setup()
        {
            Context = await new DatabaseTestUtility().SetupTimeTrackerDbContext().ConfigureAwait(false);
            Subject = new EventHistoryRepository(Context);
        }

        [Test]
        public async Task GetNextHistoryShouldReturnNullWhenNoHistoryExist()
        {
            Subject.CreateHistory(new EventHistory());
            await Context.SaveChangesAsync().ConfigureAwait(false);

            var result = await Subject.GetNextHistory(DateTime.UtcNow).ConfigureAwait(false);

            Assert.IsNull(result);
        }

        [Test]
        public async Task GetNextHistoryShouldReturnNextHistory()
        {
            Subject.CreateHistory(new EventHistory { ResourceId = 12, EventType = EventType.Task });
            Subject.CreateHistory(new EventHistory { ResourceId = 55, EventType = EventType.Interruption });
            await Context.SaveChangesAsync().ConfigureAwait(false);

            var result = await Subject.GetNextHistory(DateTime.UtcNow.AddMinutes(-5)).ConfigureAwait(false);

            Assert.AreEqual(12, result.ResourceId);
            Assert.AreEqual(EventType.Task, result.EventType);
        }

        [Test]
        public async Task GetLastHistoryShouldReturnNullWhenNoHistoryExist()
        {
            var result = await Subject.GetLastHistory().ConfigureAwait(false);

            Assert.IsNull(result);
        }

        [Test]
        public async Task GetLastHistoryShouldReturnLastHistory()
        {
            for (var i = 0; i < 3; ++i)
            {
                Subject.CreateHistory(new EventHistory());
            }

            await Context.SaveChangesAsync().ConfigureAwait(false);

            var result = await Subject.GetLastHistory().ConfigureAwait(false);

            Assert.AreEqual(3, result.Id);
        }

        [Test]
        public async Task GetHistoriesShouldReturnEmptyCollectionWhenNoHistoryFound()
        {
            var now = DateTime.UtcNow;

            for (var i = 0; i < 3; ++i)
            {
                Subject.CreateHistory(new EventHistory());
            }

            await Context.SaveChangesAsync().ConfigureAwait(false);

            var result = await Subject.GetHistories(now.AddMinutes(-10), now.AddMinutes(-5)).ConfigureAwait(false);

            Assert.IsFalse(result.Any());
        }

        [Test]
        public async Task GetHistoriesShouldReturnHistoriesFound()
        {
            var now = DateTime.UtcNow;

            for (var i = 0; i < 3; ++i)
            {
                Subject.CreateHistory(new EventHistory());
            }

            await Context.SaveChangesAsync().ConfigureAwait(false);

            var result = await Subject.GetHistories(now.AddMinutes(-5), now.AddMinutes(5)).ConfigureAwait(false);

            Assert.AreEqual(3, result.Count);
            Assert.IsTrue(result.All(_ => _.Timestamp.Kind == DateTimeKind.Utc));
        }

        [Test]
        public async Task CreateHistoryShouldReturnHistoryWhenCreationSucceeded()
        {
            var result = Subject.CreateHistory(new EventHistory());
            await Context.SaveChangesAsync().ConfigureAwait(false);

            Assert.AreEqual(1, result.Id);
            Assert.AreEqual(-1, result.TargetDuration);
            Assert.IsTrue((DateTime.UtcNow - result.Timestamp).Duration().TotalMilliseconds < 1000);
        }

        [TearDown]
        public async Task TearDown()
        {
            await Context.Database.EnsureDeletedAsync().ConfigureAwait(false);
        }
    }
}
