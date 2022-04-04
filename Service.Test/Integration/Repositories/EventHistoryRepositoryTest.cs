using Core.DbContexts;
using Core.Enums;
using Core.Models.Event;
using Core.Models.User;
using NUnit.Framework;
using Service.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Service.Test.Integration.Repositories
{
    [TestFixture]
    [Category("Integration")]
    public class EventHistoryRepositoryTest
    {
        private List<UserProfile> Users { get; set; }
        private TimeTrackerDbContext Context { get; set; }
        private EventHistoryRepository Subject { get; set; }

        [SetUp]
        public async Task Setup()
        {
            Context = await new DatabaseTestUtility().SetupTimeTrackerDbContext().ConfigureAwait(false);
            Subject = new EventHistoryRepository(Context);
            await CreateUsers().ConfigureAwait(false);
        }

        [Test]
        public async Task GetNextHistoryShouldReturnNullWhenNoHistoryExist()
        {
            Subject.CreateHistory(Users[1].Id, new EventHistory { Timestamp = DateTime.UtcNow.AddHours(-2) });
            await Context.SaveChangesAsync().ConfigureAwait(false);

            var result = await Subject.GetNextHistory(Users[0].Id, DateTime.UtcNow.AddHours(-4)).ConfigureAwait(false);

            Assert.IsNull(result);
        }

        [Test]
        public async Task GetNextHistoryShouldReturnNextHistory()
        {
            Subject.CreateHistory(Users[1].Id, new EventHistory { ResourceId = 12, EventType = EventType.Task });
            Subject.CreateHistory(Users[0].Id, new EventHistory { ResourceId = 55, EventType = EventType.Interruption });
            await Context.SaveChangesAsync().ConfigureAwait(false);

            var result = await Subject.GetNextHistory(Users[0].Id, DateTime.UtcNow.AddMinutes(-5)).ConfigureAwait(false);

            Assert.AreEqual(Users[0].Id, result.UserId);
            Assert.AreEqual(55, result.ResourceId);
            Assert.AreEqual(EventType.Interruption, result.EventType);
        }

        [Test]
        public async Task GetLastHistoryShouldReturnNullWhenNoHistoryExist()
        {
            var result = await Subject.GetLastHistory(Users[0].Id).ConfigureAwait(false);

            Assert.IsNull(result);
        }

        [Test]
        public async Task GetLastHistoryShouldReturnLastHistory()
        {
            for (var i = 0; i < 3; ++i)
            {
                Subject.CreateHistory(Users[0].Id, new EventHistory());
            }

            await Context.SaveChangesAsync().ConfigureAwait(false);

            var result = await Subject.GetLastHistory(Users[0].Id).ConfigureAwait(false);

            Assert.AreEqual(Users[0].Id, result.UserId);
            Assert.AreEqual(3, result.Id);
        }

        [Test]
        public async Task GetHistoriesShouldReturnEmptyCollectionWhenNoHistoryFound()
        {
            var now = DateTime.UtcNow;

            for (var i = 0; i < 3; ++i)
            {
                Subject.CreateHistory(Users[0].Id, new EventHistory());
            }

            await Context.SaveChangesAsync().ConfigureAwait(false);

            var result = await Subject.GetHistories(Users[0].Id, now.AddMinutes(-10), now.AddMinutes(-5)).ConfigureAwait(false);

            Assert.IsFalse(result.Any());
        }

        [Test]
        public async Task GetHistoriesShouldReturnHistoriesFound()
        {
            var now = DateTime.UtcNow;
            Subject.CreateHistory(Users[0].Id, new EventHistory());
            Subject.CreateHistory(Users[1].Id, new EventHistory());
            Subject.CreateHistory(Users[0].Id, new EventHistory());
            await Context.SaveChangesAsync().ConfigureAwait(false);

            var result = await Subject.GetHistories(Users[0].Id, now.AddMinutes(-5), now.AddMinutes(5)).ConfigureAwait(false);

            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.All(_ => _.UserId == Users[0].Id && _.Timestamp.Kind == DateTimeKind.Utc));
        }

        [Test]
        public async Task CreateHistoryShouldReturnHistoryWhenCreationSucceeded()
        {
            var result = Subject.CreateHistory(Users[0].Id, new EventHistory());
            await Context.SaveChangesAsync().ConfigureAwait(false);

            Assert.AreEqual(Users[0].Id, result.UserId);
            Assert.AreEqual(1, result.Id);
            Assert.AreEqual(-1, result.TargetDuration);
            Assert.IsTrue((DateTime.UtcNow - result.Timestamp).Duration().TotalMilliseconds < 1000);
        }

        [TearDown]
        public async Task TearDown()
        {
            await Context.Database.EnsureDeletedAsync().ConfigureAwait(false);
        }

        private async Task CreateUsers()
        {
            var repository = new UserProfileRepository(Context);

            Users = new List<UserProfile>
            {
                repository.CreateProfile(new UserProfile { Email = "john.doe@ymail.com", DisplayName = "John Doe" }),
                repository.CreateProfile(new UserProfile { Email = "jane.doe@ymail.com", DisplayName = "Jane Doe" })
            };

            await Context.SaveChangesAsync().ConfigureAwait(false);
        }
    }
}
