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
    [Category("Integration")]
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
        public async Task GetLastPromptShouldReturnNullWhenNoPromptExist()
        {
            var result = await Subject.GetLastPrompt(null).ConfigureAwait(false);

            Assert.IsNull(result);
        }

        [Test]
        public async Task GetLastPromptShouldReturnNullWhenNoPromptWithSpecifiedTypeExist()
        {
            for (var i = 0; i < 3; ++i)
            {
                await Subject.CreatePrompt(new EventPrompt { PromptType = PromptType.ScheduledBreak }).ConfigureAwait(false);
            }

            var result = await Subject.GetLastPrompt(PromptType.SuggestedBreak).ConfigureAwait(false);

            Assert.IsNull(result);
        }

        [Test]
        public async Task GetLastPromptShouldReturnLastPromptWhenNoTypeSpecified()
        {
            for (var i = 0; i < 4; ++i)
            {
                var type = i % 2 == 0 ? PromptType.ScheduledBreak : PromptType.SuggestedBreak;
                await Subject.CreatePrompt(new EventPrompt { PromptType = type }).ConfigureAwait(false);
            }

            var result = await Subject.GetLastPrompt(null).ConfigureAwait(false);

            Assert.AreEqual(4, result.Id);
            Assert.AreEqual(PromptType.SuggestedBreak, result.PromptType);
        }

        [Test]
        public async Task GetLastPromptShouldReturnLastPromptOfSpecifiedType()
        {
            for (var i = 0; i < 4; ++i)
            {
                var type = i % 2 == 0 ? PromptType.ScheduledBreak : PromptType.SuggestedBreak;
                await Subject.CreatePrompt(new EventPrompt { PromptType = type }).ConfigureAwait(false);
            }

            var result = await Subject.GetLastPrompt(PromptType.ScheduledBreak).ConfigureAwait(false);

            Assert.AreEqual(3, result.Id);
            Assert.AreEqual(PromptType.ScheduledBreak, result.PromptType);
            Assert.AreEqual(DateTimeKind.Utc, result.Timestamp.Kind);
        }

        [Test]
        public async Task CreatePromptShouldReturnPromptCreated()
        {
            var result = await Subject.CreatePrompt(new EventPrompt()).ConfigureAwait(false);

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
