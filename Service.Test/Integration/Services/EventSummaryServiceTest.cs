using Core.DbContexts;
using Core.Dtos;
using Core.Enums;
using Core.Interfaces.Repositories;
using Core.Interfaces.Services;
using Core.Models.Event;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Service.Repositories;
using Service.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Service.Test.Integration.Services
{
    [TestFixture]
    [Category("Integration")]
    public class EventSummaryServiceTest
    {
        private TimeTrackerDbContext Context { get; set; }
        private IEventHistoryRepository EventHistoryRepository { get; set; }
        private IEventSummaryService Subject { get; set; }

        [SetUp]
        public async Task Setup()
        {
            Context = await new DatabaseTestUtility().SetupTimeTrackerDbContext().ConfigureAwait(false);

            var provider = new ServiceCollection()
                .AddSingleton(Context)
                .AddTransient<IEventHistoryRepository, EventHistoryRepository>()
                .AddTransient<IEventHistorySummaryRepository, EventHistorySummaryRepository>()
                .AddTransient<IEventPromptRepository, EventPromptRepository>()
                .AddTransient<IEventSummaryService, EventSummaryService>()
                .BuildServiceProvider();

            EventHistoryRepository = provider.GetService<IEventHistoryRepository>();
            Subject = provider.GetService<IEventSummaryService>();
        }

        [Test]
        public async Task UpdateTimeRangeShouldProperlyInsertEventsOnExistingTimeRange()
        {
            var now = DateTime.UtcNow;
            var range = new EventTimeRangeDto { Id = 2, EventType = EventType.Task, Start = now.AddHours(-5), End = now.AddHours(-3) };

            var previous = new List<EventHistory>
            {
                new EventHistory { ResourceId = 5, EventType = EventType.Interruption, Timestamp = now.AddHours(-10) },
                new EventHistory { ResourceId = 3, EventType = EventType.Interruption, Timestamp = now.AddHours(-1) }
            };

            var expected = new List<EventHistory>
            {
                new EventHistory { ResourceId = 5, EventType = EventType.Interruption, Timestamp = now.AddHours(-10) },
                new EventHistory { ResourceId = 2, EventType = EventType.Task, Timestamp = now.AddHours(-5) },
                new EventHistory { ResourceId = 5, EventType = EventType.Interruption, Timestamp = now.AddHours(-3) },
                new EventHistory { ResourceId = 3, EventType = EventType.Interruption, Timestamp = now.AddHours(-1) }
            };

            Context.AddRange(previous);
            await Context.SaveChangesAsync().ConfigureAwait(false);

            var result = await Subject.UpdateTimeRange(range).ConfigureAwait(false);
            var histories = await EventHistoryRepository.GetHistories(DateTime.MinValue, DateTime.MaxValue).ConfigureAwait(false);

            Assert.IsTrue(result);
            AreEqual(expected, histories);
        }

        [Test]
        public async Task UpdateTimeRangeShouldProperlyInsertEventsWhenNoEventExistBeforeTargetTimeRange()
        {
            var now = DateTime.UtcNow;
            var range = new EventTimeRangeDto { Id = 2, EventType = EventType.Task, Start = now.AddHours(-10), End = now.AddHours(-5) };

            var previous = new List<EventHistory>
            {
                new EventHistory { ResourceId = 5, EventType = EventType.Interruption, Timestamp = now.AddHours(-3) },
                new EventHistory { ResourceId = -1, EventType = EventType.Break, Timestamp = now.AddHours(-1) }
            };

            var expected = new List<EventHistory>
            {
                new EventHistory { ResourceId = 2, EventType = EventType.Task, Timestamp = now.AddHours(-10) },
                new EventHistory { ResourceId = -1, EventType = EventType.Idling, Timestamp = now.AddHours(-5) },
                new EventHistory { ResourceId = 5, EventType = EventType.Interruption, Timestamp = now.AddHours(-3) },
                new EventHistory { ResourceId = -1, EventType = EventType.Break, Timestamp = now.AddHours(-1) }
            };

            Context.AddRange(previous);
            await Context.SaveChangesAsync().ConfigureAwait(false);

            var result = await Subject.UpdateTimeRange(range).ConfigureAwait(false);
            var histories = await EventHistoryRepository.GetHistories(DateTime.MinValue, DateTime.MaxValue).ConfigureAwait(false);

            Assert.IsTrue(result);
            AreEqual(expected, histories);
        }

        [Test]
        public async Task UpdateTimeRangeShouldProperlyInsertEventsWhenNoEventExistAfterTargetTimeRange()
        {
            var now = DateTime.UtcNow;
            var range = new EventTimeRangeDto { Id = 2, EventType = EventType.Task, Start = now.AddHours(-3), End = now.AddHours(-1) };

            var previous = new List<EventHistory>
            {
                new EventHistory { ResourceId = 5, EventType = EventType.Interruption, Timestamp = now.AddHours(-10) },
                new EventHistory { ResourceId = -1, EventType = EventType.Break, Timestamp = now.AddHours(-5) }
            };

            var expected = new List<EventHistory>
            {
                new EventHistory { ResourceId = 5, EventType = EventType.Interruption, Timestamp = now.AddHours(-10) },
                new EventHistory { ResourceId = -1, EventType = EventType.Break, Timestamp = now.AddHours(-5) },
                new EventHistory { ResourceId = 2, EventType = EventType.Task, Timestamp = now.AddHours(-3) },
                new EventHistory { ResourceId = -1, EventType = EventType.Break, Timestamp = now.AddHours(-1) }
            };

            Context.AddRange(previous);
            await Context.SaveChangesAsync().ConfigureAwait(false);

            var result = await Subject.UpdateTimeRange(range).ConfigureAwait(false);
            var histories = await EventHistoryRepository.GetHistories(DateTime.MinValue, DateTime.MaxValue).ConfigureAwait(false);

            Assert.IsTrue(result);
            AreEqual(expected, histories);
        }

        [Test]
        public async Task UpdateTimeRangeShouldReplaceSameTimeRange()
        {
            var now = DateTime.UtcNow;
            var range = new EventTimeRangeDto { Id = 2, EventType = EventType.Task, Start = now.AddHours(-3), End = now.AddHours(-1) };

            var previous = new List<EventHistory>
            {
                new EventHistory { ResourceId = 5, EventType = EventType.Interruption, Timestamp = now.AddHours(-3) },
                new EventHistory { ResourceId = -1, EventType = EventType.Break, Timestamp = now.AddHours(-1) }
            };

            var expected = new List<EventHistory>
            {
                new EventHistory { ResourceId = 2, EventType = EventType.Task, Timestamp = now.AddHours(-3) },
                new EventHistory { ResourceId = -1, EventType = EventType.Break, Timestamp = now.AddHours(-1) }
            };

            Context.AddRange(previous);
            await Context.SaveChangesAsync().ConfigureAwait(false);

            var result = await Subject.UpdateTimeRange(range).ConfigureAwait(false);
            var histories = await EventHistoryRepository.GetHistories(DateTime.MinValue, DateTime.MaxValue).ConfigureAwait(false);

            Assert.IsTrue(result);
            AreEqual(expected, histories);
        }

        [Test]
        public async Task UpdateTimeRangeShouldOverwriteOverlappingEvents()
        {
            var now = DateTime.UtcNow;
            var range = new EventTimeRangeDto { Id = -1, EventType = EventType.Idling, Start = now.AddHours(-9), End = now.AddHours(-2) };

            var previous = new List<EventHistory>
            {
                new EventHistory { ResourceId = 5, EventType = EventType.Task, Timestamp = now.AddHours(-10) },
                new EventHistory { ResourceId = 5, EventType = EventType.Interruption, Timestamp = now.AddHours(-8) },
                new EventHistory { ResourceId = -1, EventType = EventType.Break, Timestamp = now.AddHours(-7) },
                new EventHistory { ResourceId = 2, EventType = EventType.Interruption, Timestamp = now.AddHours(-3) },
                new EventHistory { ResourceId = 3, EventType = EventType.Task, Timestamp = now.AddHours(-1) }
            };

            var expected = new List<EventHistory>
            {
                new EventHistory { ResourceId = 5, EventType = EventType.Task, Timestamp = now.AddHours(-10) },
                new EventHistory { ResourceId = -1, EventType = EventType.Idling, Timestamp = now.AddHours(-9) },
                new EventHistory { ResourceId = 2, EventType = EventType.Interruption, Timestamp = now.AddHours(-2) },
                new EventHistory { ResourceId = 3, EventType = EventType.Task, Timestamp = now.AddHours(-1) }
            };

            Context.AddRange(previous);
            await Context.SaveChangesAsync().ConfigureAwait(false);

            var result = await Subject.UpdateTimeRange(range).ConfigureAwait(false);
            var histories = await EventHistoryRepository.GetHistories(DateTime.MinValue, DateTime.MaxValue).ConfigureAwait(false);

            Assert.IsTrue(result);
            AreEqual(expected, histories);
        }

        [Test]
        public async Task UpdateTimeRangeShouldMergeWithPreviousTimeRangeWhenPreviousEventMatchesTargetEvent()
        {
            var now = DateTime.UtcNow;
            var range = new EventTimeRangeDto { Id = 2, EventType = EventType.Task, Start = now.AddHours(-5), End = now.AddHours(-3) };

            var previous = new List<EventHistory>
            {
                new EventHistory { ResourceId = 2, EventType = EventType.Task, Timestamp = now.AddHours(-10) },
                new EventHistory { ResourceId = 3, EventType = EventType.Interruption, Timestamp = now.AddHours(-5) },
                new EventHistory { ResourceId = -1, EventType = EventType.Break, Timestamp = now.AddHours(-4) },
                new EventHistory { ResourceId = -1, EventType = EventType.Idling, Timestamp = now.AddHours(-1) }
            };

            var expected = new List<EventHistory>
            {
                new EventHistory { ResourceId = 2, EventType = EventType.Task, Timestamp = now.AddHours(-10) },
                new EventHistory { ResourceId = -1, EventType = EventType.Break, Timestamp = now.AddHours(-3) },
                new EventHistory { ResourceId = -1, EventType = EventType.Idling, Timestamp = now.AddHours(-1) }
            };

            Context.AddRange(previous);
            await Context.SaveChangesAsync().ConfigureAwait(false);

            var result = await Subject.UpdateTimeRange(range).ConfigureAwait(false);
            var histories = await EventHistoryRepository.GetHistories(DateTime.MinValue, DateTime.MaxValue).ConfigureAwait(false);

            Assert.IsTrue(result);
            AreEqual(expected, histories);
        }

        [Test]
        public async Task UpdateTimeRangeShouldMergeWithNextTimeRangeWhenNextEventMatchesTargetEvent()
        {
            var now = DateTime.UtcNow;
            var range = new EventTimeRangeDto { Id = 2, EventType = EventType.Task, Start = now.AddHours(-5), End = now.AddHours(-3) };

            var previous = new List<EventHistory>
            {
                new EventHistory { ResourceId = 3, EventType = EventType.Interruption, Timestamp = now.AddHours(-5) },
                new EventHistory { ResourceId = 2, EventType = EventType.Task, Timestamp = now.AddHours(-3) },
                new EventHistory { ResourceId = -1, EventType = EventType.Break, Timestamp = now.AddHours(-1) }
            };

            var expected = new List<EventHistory>
            {
                new EventHistory { ResourceId = 2, EventType = EventType.Task, Timestamp = now.AddHours(-5) },
                new EventHistory { ResourceId = -1, EventType = EventType.Break, Timestamp = now.AddHours(-1) }
            };

            Context.AddRange(previous);
            await Context.SaveChangesAsync().ConfigureAwait(false);

            var result = await Subject.UpdateTimeRange(range).ConfigureAwait(false);
            var histories = await EventHistoryRepository.GetHistories(DateTime.MinValue, DateTime.MaxValue).ConfigureAwait(false);

            Assert.IsTrue(result);
            AreEqual(expected, histories);
        }

        [Test]
        public async Task UpdateTimeRangeShouldMergeAllTimeRangesWhenBothEventBeforeAndAfterMatchTargetEvent()
        {
            var now = DateTime.UtcNow;
            var range = new EventTimeRangeDto { Id = 2, EventType = EventType.Task, Start = now.AddHours(-5), End = now.AddHours(-3) };

            var previous = new List<EventHistory>
            {
                new EventHistory { ResourceId = 2, EventType = EventType.Task, Timestamp = now.AddHours(-10) },
                new EventHistory { ResourceId = 3, EventType = EventType.Interruption, Timestamp = now.AddHours(-4) },
                new EventHistory { ResourceId = 2, EventType = EventType.Task, Timestamp = now.AddHours(-3) }
            };

            var expected = new List<EventHistory>
            {
                new EventHistory { ResourceId = 2, EventType = EventType.Task, Timestamp = now.AddHours(-10) }
            };

            Context.AddRange(previous);
            await Context.SaveChangesAsync().ConfigureAwait(false);

            var result = await Subject.UpdateTimeRange(range).ConfigureAwait(false);
            var histories = await EventHistoryRepository.GetHistories(DateTime.MinValue, DateTime.MaxValue).ConfigureAwait(false);

            Assert.IsTrue(result);
            AreEqual(expected, histories);
        }

        [Test]
        public async Task UpdateTimeRangeShouldDoNothingWhenIdlingTimeIsUnchanged()
        {
            var now = DateTime.UtcNow;
            var range = new EventTimeRangeDto { Id = -1, EventType = EventType.Idling, Start = now.AddHours(-5), End = now.AddHours(-3) };

            var previous = new List<EventHistory>
            {
                new EventHistory { ResourceId = -1, EventType = EventType.Idling, Timestamp = now.AddHours(-10) },
                new EventHistory { ResourceId = 2, EventType = EventType.Task, Timestamp = now.AddHours(-3) }
            };

            Context.AddRange(previous);
            await Context.SaveChangesAsync().ConfigureAwait(false);

            var result = await Subject.UpdateTimeRange(range).ConfigureAwait(false);
            var histories = await EventHistoryRepository.GetHistories(DateTime.MinValue, DateTime.MaxValue).ConfigureAwait(false);

            Assert.IsTrue(result);
            AreEqual(previous, histories);
        }

        [Test]
        public async Task UpdateTimeRangeShouldUpdateTimeRangeForSameEventWhenStartTimeIsTheSame()
        {
            var now = DateTime.UtcNow;
            var range = new EventTimeRangeDto { Id = -1, EventType = EventType.Break, Start = now.AddHours(-5), End = now.AddHours(-3) };

            var previous = new List<EventHistory>
            {
                new EventHistory { ResourceId = -1, EventType = EventType.Break, Timestamp = now.AddHours(-5) },
                new EventHistory { ResourceId = 3, EventType = EventType.Interruption, Timestamp = now.AddHours(-1) }
            };

            var expected = new List<EventHistory>
            {
                new EventHistory { ResourceId = -1, EventType = EventType.Break, Timestamp = now.AddHours(-5) },
                new EventHistory { ResourceId = -1, EventType = EventType.Idling, Timestamp = now.AddHours(-3) },
                new EventHistory { ResourceId = 3, EventType = EventType.Interruption, Timestamp = now.AddHours(-1) }
            };

            Context.AddRange(previous);
            await Context.SaveChangesAsync().ConfigureAwait(false);

            var result = await Subject.UpdateTimeRange(range).ConfigureAwait(false);
            var histories = await EventHistoryRepository.GetHistories(DateTime.MinValue, DateTime.MaxValue).ConfigureAwait(false);

            Assert.IsTrue(result);
            AreEqual(expected, histories);
        }

        [Test]
        public async Task UpdateTimeRangeShouldUpdateTimeRangeForSameEventWhenEndTimeIsTheSame()
        {
            var now = DateTime.UtcNow;
            var range = new EventTimeRangeDto { Id = 2, EventType = EventType.Task, Start = now.AddHours(-5), End = now.AddHours(-3) };

            var previous = new List<EventHistory>
            {
                new EventHistory { ResourceId = -1, EventType = EventType.Break, Timestamp = now.AddHours(-12) },
                new EventHistory { ResourceId = 2, EventType = EventType.Task, Timestamp = now.AddHours(-10) },
                new EventHistory { ResourceId = 3, EventType = EventType.Interruption, Timestamp = now.AddHours(-3) }
            };

            var expected = new List<EventHistory>
            {
                new EventHistory { ResourceId = -1, EventType = EventType.Break, Timestamp = now.AddHours(-12) },
                new EventHistory { ResourceId = -1, EventType = EventType.Idling, Timestamp = now.AddHours(-10) },
                new EventHistory { ResourceId = 2, EventType = EventType.Task, Timestamp = now.AddHours(-5) },
                new EventHistory { ResourceId = 3, EventType = EventType.Interruption, Timestamp = now.AddHours(-3) }
            };

            Context.AddRange(previous);
            await Context.SaveChangesAsync().ConfigureAwait(false);

            var result = await Subject.UpdateTimeRange(range).ConfigureAwait(false);
            var histories = await EventHistoryRepository.GetHistories(DateTime.MinValue, DateTime.MaxValue).ConfigureAwait(false);

            Assert.IsTrue(result);
            AreEqual(expected, histories);
        }

        [Test]
        public async Task UpdateTimeRangeShouldUpdateTimeRangeForSameEventWhenNeitherStartTimeNorEndTimeIsTheSame()
        {
            var now = DateTime.UtcNow;
            var range = new EventTimeRangeDto { Id = 5, EventType = EventType.Interruption, Start = now.AddHours(-5), End = now.AddHours(-3) };

            var previous = new List<EventHistory>
            {
                new EventHistory { ResourceId = -1, EventType = EventType.Idling, Timestamp = now.AddHours(-12) },
                new EventHistory { ResourceId = 5, EventType = EventType.Interruption, Timestamp = now.AddHours(-10) },
                new EventHistory { ResourceId = 9, EventType = EventType.Task, Timestamp = now.AddHours(-1) }
            };

            var expected = new List<EventHistory>
            {
                new EventHistory { ResourceId = -1, EventType = EventType.Idling, Timestamp = now.AddHours(-12) },
                new EventHistory { ResourceId = 5, EventType = EventType.Interruption, Timestamp = now.AddHours(-5) },
                new EventHistory { ResourceId = -1, EventType = EventType.Idling, Timestamp = now.AddHours(-3) },
                new EventHistory { ResourceId = 9, EventType = EventType.Task, Timestamp = now.AddHours(-1) }
            };

            Context.AddRange(previous);
            await Context.SaveChangesAsync().ConfigureAwait(false);

            var result = await Subject.UpdateTimeRange(range).ConfigureAwait(false);
            var histories = await EventHistoryRepository.GetHistories(DateTime.MinValue, DateTime.MaxValue).ConfigureAwait(false);

            Assert.IsTrue(result);
            AreEqual(expected, histories);
        }

        [TearDown]
        public async Task TearDown()
        {
            await Context.Database.EnsureDeletedAsync().ConfigureAwait(false);
        }

        private void AreEqual(List<EventHistory> expected, List<EventHistory> actual)
        {
            Assert.AreEqual(expected.Count, actual.Count);

            for (var i = 0; i < expected.Count; ++i)
            {
                Assert.AreEqual(expected[i].ResourceId, actual[i].ResourceId);
                Assert.AreEqual(expected[i].EventType, actual[i].EventType);
                Assert.AreEqual(expected[i].Timestamp, actual[i].Timestamp);
            }
        }
    }
}
