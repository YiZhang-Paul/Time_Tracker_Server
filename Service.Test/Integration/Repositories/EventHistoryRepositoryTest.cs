using Core.DbContexts;
using Core.Models.Event;
using NUnit.Framework;
using Service.Repositories;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Service.Test.Integration.Repositories
{
    [TestFixture]
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
        public async Task GetLastEventHistoryShouldReturnNullWhenNoEventHistoryExist()
        {
            var result = await Subject.GetLastEventHistory().ConfigureAwait(false);

            Assert.IsNull(result);
        }

        [Test]
        public async Task GetLastEventHistoryShouldReturnLastEventHistory()
        {
            for (var i = 0; i < 3; ++i)
            {
                await Subject.CreateEventHistory(new EventHistory()).ConfigureAwait(false);
            }

            var result = await Subject.GetLastEventHistory().ConfigureAwait(false);

            Assert.AreEqual(3, result.Id);
        }

        [Test]
        public async Task GetEventHistoryByIdShouldReturnNullWhenNoEventHistoryFound()
        {
            await Subject.CreateEventHistory(new EventHistory { Id = 5 }).ConfigureAwait(false);

            var result = await Subject.GetEventHistoryById(4).ConfigureAwait(false);

            Assert.IsNull(result);
        }

        [Test]
        public async Task GetEventHistoryByIdShouldReturnEventHistoryFound()
        {
            await Subject.CreateEventHistory(new EventHistory { Id = 5 }).ConfigureAwait(false);

            var result = await Subject.GetEventHistoryById(5).ConfigureAwait(false);

            Assert.AreEqual(5, result.Id);
        }

        [Test]
        public async Task GetEventHistoriesShouldReturnEmptyCollectionWhenNoEventHistoriesFound()
        {
            var now = DateTime.UtcNow;

            for (var i = 0; i < 3; ++i)
            {
                await Subject.CreateEventHistory(new EventHistory()).ConfigureAwait(false);
            }

            var result = await Subject.GetEventHistories(now.AddMinutes(-10), now.AddMinutes(-5)).ConfigureAwait(false);

            Assert.IsFalse(result.Any());
        }

        [Test]
        public async Task GetEventHistoriesShouldReturnEventHistoriesFound()
        {
            var now = DateTime.UtcNow;

            for (var i = 0; i < 3; ++i)
            {
                await Subject.CreateEventHistory(new EventHistory()).ConfigureAwait(false);
            }

            var result = await Subject.GetEventHistories(now.AddMinutes(-5), now.AddMinutes(5)).ConfigureAwait(false);

            Assert.AreEqual(3, result.Count);
        }

        [Test]
        public async Task CreateEventHistoryShouldReturnEventHistoryWhenCreationSucceeded()
        {
            var result = await Subject.CreateEventHistory(new EventHistory()).ConfigureAwait(false);

            Assert.AreEqual(1, result.Id);
            Assert.IsTrue((DateTime.UtcNow - result.Timestamp).Duration().TotalMilliseconds < 1000);
        }

        [TearDown]
        public async Task TearDown()
        {
            await Context.Database.EnsureDeletedAsync().ConfigureAwait(false);
        }
    }
}
