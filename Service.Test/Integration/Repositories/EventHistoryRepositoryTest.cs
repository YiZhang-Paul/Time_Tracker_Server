using Core.DbContexts;
using Core.Models.Event;
using NUnit.Framework;
using Service.Repositories;
using System;
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
