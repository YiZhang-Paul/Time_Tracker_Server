using Core.DbContexts;
using Core.Enums;
using Core.Models.Event;
using NUnit.Framework;
using Service.Repositories;
using System;
using System.Threading.Tasks;

namespace Service.Test.Integration.Repositories
{
    [TestFixture]
    public class EventPromptRepositoryTest
    {
        private TimeTrackerDbContext Context { get; set; }
        private EventPromptRepository Subject { get; set; }

        [SetUp]
        public async Task Setup()
        {
            Context = await new DatabaseTestUtility().SetupTimeTrackerDbContext().ConfigureAwait(false);
            Subject = new EventPromptRepository(Context);
        }

        [Test]
        public async Task GetLastEventPromptShouldReturnNullWhenNoEventPromptExist()
        {
            var result = await Subject.GetLastEventPrompt(null).ConfigureAwait(false);

            Assert.IsNull(result);
        }

        [Test]
        public async Task GetLastEventPromptShouldReturnNullWhenNoEventPromptWithSpecifiedTypeExist()
        {
            for (var i = 0; i < 3; ++i)
            {
                await Subject.CreateEventPrompt(new EventPrompt { PromptType = PromptType.ScheduledBreak }).ConfigureAwait(false);
            }

            var result = await Subject.GetLastEventPrompt(PromptType.SuggestedBreak).ConfigureAwait(false);

            Assert.IsNull(result);
        }

        [Test]
        public async Task GetLastEventPromptShouldReturnLastEventPromptWhenNoTypeSpecified()
        {
            for (var i = 0; i < 4; ++i)
            {
                var type = i % 2 == 0 ? PromptType.ScheduledBreak : PromptType.SuggestedBreak;
                await Subject.CreateEventPrompt(new EventPrompt { PromptType = type }).ConfigureAwait(false);
            }

            var result = await Subject.GetLastEventPrompt(null).ConfigureAwait(false);

            Assert.AreEqual(4, result.Id);
            Assert.AreEqual(PromptType.SuggestedBreak, result.PromptType);
        }

        [Test]
        public async Task GetLastEventPromptShouldReturnLastEventPromptOfSpecifiedType()
        {
            for (var i = 0; i < 4; ++i)
            {
                var type = i % 2 == 0 ? PromptType.ScheduledBreak : PromptType.SuggestedBreak;
                await Subject.CreateEventPrompt(new EventPrompt { PromptType = type }).ConfigureAwait(false);
            }

            var result = await Subject.GetLastEventPrompt(PromptType.ScheduledBreak).ConfigureAwait(false);

            Assert.AreEqual(3, result.Id);
            Assert.AreEqual(PromptType.ScheduledBreak, result.PromptType);
        }

        [Test]
        public async Task CreateEventPromptShouldReturnEventPromptWhenCreationSucceeded()
        {
            var result = await Subject.CreateEventPrompt(new EventPrompt()).ConfigureAwait(false);

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
