using Core.DbContexts;
using Core.Enums;
using Core.Models.Authentication;
using Core.Models.Event;
using NUnit.Framework;
using Service.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Service.Test.Integration.Repositories
{
    [TestFixture]
    [Category("Integration")]
    public class EventPromptRepositoryTest
    {
        private List<UserProfile> Users { get; set; }
        private TimeTrackerDbContext Context { get; set; }
        private EventPromptRepository Subject { get; set; }

        [SetUp]
        public async Task Setup()
        {
            Context = await new DatabaseTestUtility().SetupTimeTrackerDbContext().ConfigureAwait(false);
            Subject = new EventPromptRepository(Context);
            await CreateUsers().ConfigureAwait(false);
        }

        [Test]
        public async Task GetLastPromptShouldReturnNullWhenNoPromptExist()
        {
            var result = await Subject.GetLastPrompt(Users[0].Id, null).ConfigureAwait(false);

            Assert.IsNull(result);
        }

        [Test]
        public async Task GetLastPromptShouldReturnNullWhenNoPromptWithSpecifiedTypeExist()
        {
            for (var i = 0; i < 3; ++i)
            {
                Subject.CreatePrompt(Users[0].Id, new EventPrompt { PromptType = PromptType.ScheduledBreak });
            }

            await Context.SaveChangesAsync().ConfigureAwait(false);

            var result = await Subject.GetLastPrompt(Users[0].Id, PromptType.SuggestedBreak).ConfigureAwait(false);

            Assert.IsNull(result);
        }

        [Test]
        public async Task GetLastPromptShouldReturnLastPromptWhenNoTypeSpecified()
        {
            Subject.CreatePrompt(Users[0].Id, new EventPrompt { PromptType = PromptType.ScheduledBreak });
            Subject.CreatePrompt(Users[0].Id, new EventPrompt { PromptType = PromptType.SuggestedBreak });
            Subject.CreatePrompt(Users[1].Id, new EventPrompt { PromptType = PromptType.ScheduledBreak });
            Subject.CreatePrompt(Users[1].Id, new EventPrompt { PromptType = PromptType.SuggestedBreak });
            await Context.SaveChangesAsync().ConfigureAwait(false);

            var result = await Subject.GetLastPrompt(Users[0].Id, null).ConfigureAwait(false);

            Assert.AreEqual(Users[0].Id, result.UserId);
            Assert.AreEqual(2, result.Id);
            Assert.AreEqual(PromptType.SuggestedBreak, result.PromptType);
        }

        [Test]
        public async Task GetLastPromptShouldReturnLastPromptOfSpecifiedType()
        {
            Subject.CreatePrompt(Users[0].Id, new EventPrompt { PromptType = PromptType.ScheduledBreak });
            Subject.CreatePrompt(Users[0].Id, new EventPrompt { PromptType = PromptType.SuggestedBreak });
            Subject.CreatePrompt(Users[1].Id, new EventPrompt { PromptType = PromptType.ScheduledBreak });
            Subject.CreatePrompt(Users[1].Id, new EventPrompt { PromptType = PromptType.SuggestedBreak });
            await Context.SaveChangesAsync().ConfigureAwait(false);

            var result = await Subject.GetLastPrompt(Users[0].Id, PromptType.ScheduledBreak).ConfigureAwait(false);

            Assert.AreEqual(Users[0].Id, result.UserId);
            Assert.AreEqual(1, result.Id);
            Assert.AreEqual(PromptType.ScheduledBreak, result.PromptType);
            Assert.AreEqual(DateTimeKind.Utc, result.Timestamp.Kind);
        }

        [Test]
        public async Task CreatePromptShouldReturnPromptCreated()
        {
            var prompt = new EventPrompt { PromptType = PromptType.ScheduledBreak, ConfirmType = PromptConfirmType.Commenced };

            var result = Subject.CreatePrompt(Users[0].Id, prompt);
            await Context.SaveChangesAsync().ConfigureAwait(false);

            Assert.AreEqual(Users[0].Id, result.UserId);
            Assert.AreEqual(1, result.Id);
            Assert.AreEqual(PromptType.ScheduledBreak, result.PromptType);
            Assert.AreEqual(PromptConfirmType.Commenced, result.ConfirmType);
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
