using Core.DbContexts;
using Core.Dtos;
using Core.Enums;
using Core.Interfaces.Repositories;
using Core.Interfaces.UnitOfWorks;
using Core.Models.Event;
using Core.Models.User;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Service.Repositories;
using Service.Services;
using Service.UnitOfWorks;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Service.Test.Integration.Services
{
    [TestFixture]
    [Category("Integration")]
    public class EventTrackingServiceTest
    {
        private List<UserProfile> Users { get; set; }
        private TimeTrackerDbContext Context { get; set; }
        private IEventUnitOfWork EventUnitOfWork { get; set; }
        private IEventTrackingService Subject { get; set; }

        [SetUp]
        public async Task Setup()
        {
            Context = await new DatabaseTestUtility().SetupTimeTrackerDbContext().ConfigureAwait(false);

            var provider = new ServiceCollection()
                .AddSingleton(Context)
                .AddTransient<IEventHistoryRepository, EventHistoryRepository>()
                .AddTransient<IEventHistorySummaryRepository, EventHistorySummaryRepository>()
                .AddTransient<IEventPromptRepository, EventPromptRepository>()
                .AddTransient<IEventUnitOfWork, EventUnitOfWork>()
                .AddTransient<IEventTrackingService, EventTrackingService>()
                .BuildServiceProvider();

            EventUnitOfWork = provider.GetService<IEventUnitOfWork>();
            Subject = provider.GetService<IEventTrackingService>();
            await CreateUsers().ConfigureAwait(false);
        }

        [Test]
        public async Task UpdateTimeRangeShouldProperlyInsertEventsOnExistingTimeRange()
        {
            var now = DateTime.UtcNow;
            var range = new EventTimeRangeDto { Id = 2, EventType = EventType.Task, Start = now.AddHours(-5), End = now.AddHours(-3) };

            var previous = new List<EventHistory>
            {
                new EventHistory { UserId = Users[0].Id, ResourceId = 5, EventType = EventType.Interruption, Timestamp = now.AddHours(-10) },
                new EventHistory { UserId = Users[1].Id, ResourceId = 6, EventType = EventType.Interruption, Timestamp = now.AddHours(-8) },
                new EventHistory { UserId = Users[0].Id, ResourceId = 3, EventType = EventType.Interruption, Timestamp = now.AddHours(-1) },
                new EventHistory { UserId = Users[1].Id, ResourceId = 9, EventType = EventType.Interruption, Timestamp = now.AddHours(-0.5) }
            };

            var expected1 = new List<EventHistory>
            {
                new EventHistory { UserId = Users[0].Id, ResourceId = 5, EventType = EventType.Interruption, Timestamp = now.AddHours(-10) },
                new EventHistory { UserId = Users[0].Id, ResourceId = 2, EventType = EventType.Task, Timestamp = now.AddHours(-5) },
                new EventHistory { UserId = Users[0].Id, ResourceId = 5, EventType = EventType.Interruption, Timestamp = now.AddHours(-3) },
                new EventHistory { UserId = Users[0].Id, ResourceId = 3, EventType = EventType.Interruption, Timestamp = now.AddHours(-1) }
            };

            var expected2 = new List<EventHistory>
            {
                new EventHistory { UserId = Users[1].Id, ResourceId = 6, EventType = EventType.Interruption, Timestamp = now.AddHours(-8) },
                new EventHistory { UserId = Users[1].Id, ResourceId = 9, EventType = EventType.Interruption, Timestamp = now.AddHours(-0.5) }
            };

            Context.AddRange(previous);
            await Context.SaveChangesAsync().ConfigureAwait(false);

            var result = await Subject.UpdateTimeRange(Users[0].Id, range).ConfigureAwait(false);
            var histories1 = await EventUnitOfWork.EventHistory.GetHistories(Users[0].Id, DateTime.MinValue, DateTime.MaxValue).ConfigureAwait(false);
            var histories2 = await EventUnitOfWork.EventHistory.GetHistories(Users[1].Id, DateTime.MinValue, DateTime.MaxValue).ConfigureAwait(false);

            Assert.IsTrue(result);
            AreEqual(expected1, histories1);
            AreEqual(expected2, histories2);
        }

        [Test]
        public async Task UpdateTimeRangeShouldProperlyInsertEventsWhenNoEventExistBeforeTargetTimeRange()
        {
            var now = DateTime.UtcNow;
            var range = new EventTimeRangeDto { Id = 2, EventType = EventType.Task, Start = now.AddHours(-10), End = now.AddHours(-5) };

            var previous = new List<EventHistory>
            {
                new EventHistory { UserId = Users[0].Id, ResourceId = 5, EventType = EventType.Interruption, Timestamp = now.AddHours(-3) },
                new EventHistory { UserId = Users[1].Id, ResourceId = 3, EventType = EventType.Task, Timestamp = now.AddHours(-2) },
                new EventHistory { UserId = Users[0].Id, ResourceId = -1, EventType = EventType.Break, Timestamp = now.AddHours(-1) },
                new EventHistory { UserId = Users[1].Id, ResourceId = -1, EventType = EventType.Break, Timestamp = now.AddHours(-1) }
            };

            var expected1 = new List<EventHistory>
            {
                new EventHistory { UserId = Users[0].Id, ResourceId = 2, EventType = EventType.Task, Timestamp = now.AddHours(-10) },
                new EventHistory { UserId = Users[0].Id, ResourceId = -1, EventType = EventType.Idling, Timestamp = now.AddHours(-5) },
                new EventHistory { UserId = Users[0].Id, ResourceId = 5, EventType = EventType.Interruption, Timestamp = now.AddHours(-3) },
                new EventHistory { UserId = Users[0].Id, ResourceId = -1, EventType = EventType.Break, Timestamp = now.AddHours(-1) }
            };

            var expected2 = new List<EventHistory>
            {
                new EventHistory { UserId = Users[1].Id, ResourceId = 3, EventType = EventType.Task, Timestamp = now.AddHours(-2) },
                new EventHistory { UserId = Users[1].Id, ResourceId = -1, EventType = EventType.Break, Timestamp = now.AddHours(-1) }
            };

            Context.AddRange(previous);
            await Context.SaveChangesAsync().ConfigureAwait(false);

            var result = await Subject.UpdateTimeRange(Users[0].Id, range).ConfigureAwait(false);
            var histories1 = await EventUnitOfWork.EventHistory.GetHistories(Users[0].Id, DateTime.MinValue, DateTime.MaxValue).ConfigureAwait(false);
            var histories2 = await EventUnitOfWork.EventHistory.GetHistories(Users[1].Id, DateTime.MinValue, DateTime.MaxValue).ConfigureAwait(false);

            Assert.IsTrue(result);
            AreEqual(expected1, histories1);
            AreEqual(expected2, histories2);
        }

        [Test]
        public async Task UpdateTimeRangeShouldProperlyInsertEventsWhenNoEventExistAfterTargetTimeRange()
        {
            var now = DateTime.UtcNow;
            var range = new EventTimeRangeDto { Id = 2, EventType = EventType.Task, Start = now.AddHours(-3), End = now.AddHours(-1) };

            var previous = new List<EventHistory>
            {
                new EventHistory { UserId = Users[0].Id, ResourceId = 5, EventType = EventType.Interruption, Timestamp = now.AddHours(-10) },
                new EventHistory { UserId = Users[1].Id, ResourceId = 9, EventType = EventType.Interruption, Timestamp = now.AddHours(-10) },
                new EventHistory { UserId = Users[1].Id, ResourceId = -1, EventType = EventType.Break, Timestamp = now.AddHours(-6) },
                new EventHistory { UserId = Users[0].Id, ResourceId = -1, EventType = EventType.Break, Timestamp = now.AddHours(-5) }
            };

            var expected1 = new List<EventHistory>
            {
                new EventHistory { UserId = Users[0].Id, ResourceId = 5, EventType = EventType.Interruption, Timestamp = now.AddHours(-10) },
                new EventHistory { UserId = Users[0].Id, ResourceId = -1, EventType = EventType.Break, Timestamp = now.AddHours(-5) },
                new EventHistory { UserId = Users[0].Id, ResourceId = 2, EventType = EventType.Task, Timestamp = now.AddHours(-3) },
                new EventHistory { UserId = Users[0].Id, ResourceId = -1, EventType = EventType.Break, Timestamp = now.AddHours(-1) }
            };

            var expected2 = new List<EventHistory>
            {
                new EventHistory { UserId = Users[1].Id, ResourceId = 9, EventType = EventType.Interruption, Timestamp = now.AddHours(-10) },
                new EventHistory { UserId = Users[1].Id, ResourceId = -1, EventType = EventType.Break, Timestamp = now.AddHours(-6) }
            };

            Context.AddRange(previous);
            await Context.SaveChangesAsync().ConfigureAwait(false);

            var result = await Subject.UpdateTimeRange(Users[0].Id, range).ConfigureAwait(false);
            var histories1 = await EventUnitOfWork.EventHistory.GetHistories(Users[0].Id, DateTime.MinValue, DateTime.MaxValue).ConfigureAwait(false);
            var histories2 = await EventUnitOfWork.EventHistory.GetHistories(Users[1].Id, DateTime.MinValue, DateTime.MaxValue).ConfigureAwait(false);

            Assert.IsTrue(result);
            AreEqual(expected1, histories1);
            AreEqual(expected2, histories2);
        }

        [Test]
        public async Task UpdateTimeRangeShouldReplaceSameTimeRange()
        {
            var now = DateTime.UtcNow;
            var range = new EventTimeRangeDto { Id = 2, EventType = EventType.Task, Start = now.AddHours(-3), End = now.AddHours(-1) };

            var previous = new List<EventHistory>
            {
                new EventHistory { UserId = Users[0].Id, ResourceId = 5, EventType = EventType.Interruption, Timestamp = now.AddHours(-3) },
                new EventHistory { UserId = Users[1].Id, ResourceId = 3, EventType = EventType.Task, Timestamp = now.AddHours(-2) },
                new EventHistory { UserId = Users[0].Id, ResourceId = -1, EventType = EventType.Break, Timestamp = now.AddHours(-1) }
            };

            var expected1 = new List<EventHistory>
            {
                new EventHistory { UserId = Users[0].Id, ResourceId = 2, EventType = EventType.Task, Timestamp = now.AddHours(-3) },
                new EventHistory { UserId = Users[0].Id, ResourceId = -1, EventType = EventType.Break, Timestamp = now.AddHours(-1) }
            };

            var expected2 = new List<EventHistory>
            {
                new EventHistory { UserId = Users[1].Id, ResourceId = 3, EventType = EventType.Task, Timestamp = now.AddHours(-2) }
            };

            Context.AddRange(previous);
            await Context.SaveChangesAsync().ConfigureAwait(false);

            var result = await Subject.UpdateTimeRange(Users[0].Id, range).ConfigureAwait(false);
            var histories1 = await EventUnitOfWork.EventHistory.GetHistories(Users[0].Id, DateTime.MinValue, DateTime.MaxValue).ConfigureAwait(false);
            var histories2 = await EventUnitOfWork.EventHistory.GetHistories(Users[1].Id, DateTime.MinValue, DateTime.MaxValue).ConfigureAwait(false);

            Assert.IsTrue(result);
            AreEqual(expected1, histories1);
            AreEqual(expected2, histories2);
        }

        [Test]
        public async Task UpdateTimeRangeShouldOverwriteOverlappingEvents()
        {
            var now = DateTime.UtcNow;
            var range = new EventTimeRangeDto { Id = -1, EventType = EventType.Idling, Start = now.AddHours(-9), End = now.AddHours(-2) };

            var previous = new List<EventHistory>
            {
                new EventHistory { UserId = Users[0].Id, ResourceId = 5, EventType = EventType.Task, Timestamp = now.AddHours(-10) },
                new EventHistory { UserId = Users[1].Id, ResourceId = 7, EventType = EventType.Task, Timestamp = now.AddHours(-10) },
                new EventHistory { UserId = Users[0].Id, ResourceId = 5, EventType = EventType.Interruption, Timestamp = now.AddHours(-8) },
                new EventHistory { UserId = Users[0].Id, ResourceId = -1, EventType = EventType.Break, Timestamp = now.AddHours(-7) },
                new EventHistory { UserId = Users[1].Id, ResourceId = -1, EventType = EventType.Break, Timestamp = now.AddHours(-5) },
                new EventHistory { UserId = Users[0].Id, ResourceId = 2, EventType = EventType.Interruption, Timestamp = now.AddHours(-3) },
                new EventHistory { UserId = Users[1].Id, ResourceId = 9, EventType = EventType.Task, Timestamp = now.AddHours(-2) },
                new EventHistory { UserId = Users[0].Id, ResourceId = 3, EventType = EventType.Task, Timestamp = now.AddHours(-1) }
            };

            var expected1 = new List<EventHistory>
            {
                new EventHistory { UserId = Users[0].Id, ResourceId = 5, EventType = EventType.Task, Timestamp = now.AddHours(-10) },
                new EventHistory { UserId = Users[0].Id, ResourceId = -1, EventType = EventType.Idling, Timestamp = now.AddHours(-9) },
                new EventHistory { UserId = Users[0].Id, ResourceId = 2, EventType = EventType.Interruption, Timestamp = now.AddHours(-2) },
                new EventHistory { UserId = Users[0].Id, ResourceId = 3, EventType = EventType.Task, Timestamp = now.AddHours(-1) }
            };

            var expected2 = new List<EventHistory>
            {
                new EventHistory { UserId = Users[1].Id, ResourceId = 7, EventType = EventType.Task, Timestamp = now.AddHours(-10) },
                new EventHistory { UserId = Users[1].Id, ResourceId = -1, EventType = EventType.Break, Timestamp = now.AddHours(-5) },
                new EventHistory { UserId = Users[1].Id, ResourceId = 9, EventType = EventType.Task, Timestamp = now.AddHours(-2) }
            };

            Context.AddRange(previous);
            await Context.SaveChangesAsync().ConfigureAwait(false);

            var result = await Subject.UpdateTimeRange(Users[0].Id, range).ConfigureAwait(false);
            var histories1 = await EventUnitOfWork.EventHistory.GetHistories(Users[0].Id, DateTime.MinValue, DateTime.MaxValue).ConfigureAwait(false);
            var histories2 = await EventUnitOfWork.EventHistory.GetHistories(Users[1].Id, DateTime.MinValue, DateTime.MaxValue).ConfigureAwait(false);

            Assert.IsTrue(result);
            AreEqual(expected1, histories1);
            AreEqual(expected2, histories2);
        }

        [Test]
        public async Task UpdateTimeRangeShouldMergeWithPreviousTimeRangeWhenPreviousEventMatchesTargetEvent()
        {
            var now = DateTime.UtcNow;
            var range = new EventTimeRangeDto { Id = 2, EventType = EventType.Task, Start = now.AddHours(-5), End = now.AddHours(-3) };

            var previous = new List<EventHistory>
            {
                new EventHistory { UserId = Users[1].Id, ResourceId = 5, EventType = EventType.Task, Timestamp = now.AddHours(-12) },
                new EventHistory { UserId = Users[0].Id, ResourceId = 2, EventType = EventType.Task, Timestamp = now.AddHours(-10) },
                new EventHistory { UserId = Users[0].Id, ResourceId = 3, EventType = EventType.Interruption, Timestamp = now.AddHours(-5) },
                new EventHistory { UserId = Users[1].Id, ResourceId = 7, EventType = EventType.Interruption, Timestamp = now.AddHours(-5) },
                new EventHistory { UserId = Users[0].Id, ResourceId = -1, EventType = EventType.Break, Timestamp = now.AddHours(-4) },
                new EventHistory { UserId = Users[0].Id, ResourceId = -1, EventType = EventType.Idling, Timestamp = now.AddHours(-1) }
            };

            var expected1 = new List<EventHistory>
            {
                new EventHistory { UserId = Users[0].Id, ResourceId = 2, EventType = EventType.Task, Timestamp = now.AddHours(-10) },
                new EventHistory { UserId = Users[0].Id, ResourceId = -1, EventType = EventType.Break, Timestamp = now.AddHours(-3) },
                new EventHistory { UserId = Users[0].Id, ResourceId = -1, EventType = EventType.Idling, Timestamp = now.AddHours(-1) }
            };

            var expected2 = new List<EventHistory>
            {
                new EventHistory { UserId = Users[1].Id, ResourceId = 5, EventType = EventType.Task, Timestamp = now.AddHours(-12) },
                new EventHistory { UserId = Users[1].Id, ResourceId = 7, EventType = EventType.Interruption, Timestamp = now.AddHours(-5) }
            };

            Context.AddRange(previous);
            await Context.SaveChangesAsync().ConfigureAwait(false);

            var result = await Subject.UpdateTimeRange(Users[0].Id, range).ConfigureAwait(false);
            var histories1 = await EventUnitOfWork.EventHistory.GetHistories(Users[0].Id, DateTime.MinValue, DateTime.MaxValue).ConfigureAwait(false);
            var histories2 = await EventUnitOfWork.EventHistory.GetHistories(Users[1].Id, DateTime.MinValue, DateTime.MaxValue).ConfigureAwait(false);

            Assert.IsTrue(result);
            AreEqual(expected1, histories1);
            AreEqual(expected2, histories2);
        }

        [Test]
        public async Task UpdateTimeRangeShouldMergeWithNextTimeRangeWhenNextEventMatchesTargetEvent()
        {
            var now = DateTime.UtcNow;
            var range = new EventTimeRangeDto { Id = 2, EventType = EventType.Task, Start = now.AddHours(-5), End = now.AddHours(-3) };

            var previous = new List<EventHistory>
            {
                new EventHistory { UserId = Users[1].Id, ResourceId = 9, EventType = EventType.Interruption, Timestamp = now.AddHours(-5) },
                new EventHistory { UserId = Users[0].Id, ResourceId = 3, EventType = EventType.Interruption, Timestamp = now.AddHours(-5) },
                new EventHistory { UserId = Users[0].Id, ResourceId = 2, EventType = EventType.Task, Timestamp = now.AddHours(-3) },
                new EventHistory { UserId = Users[1].Id, ResourceId = -1, EventType = EventType.Break, Timestamp = now.AddHours(-2) },
                new EventHistory { UserId = Users[0].Id, ResourceId = -1, EventType = EventType.Break, Timestamp = now.AddHours(-1) }
            };

            var expected1 = new List<EventHistory>
            {
                new EventHistory { UserId = Users[0].Id, ResourceId = 2, EventType = EventType.Task, Timestamp = now.AddHours(-5) },
                new EventHistory { UserId = Users[0].Id, ResourceId = -1, EventType = EventType.Break, Timestamp = now.AddHours(-1) }
            };

            var expected2 = new List<EventHistory>
            {
                new EventHistory { UserId = Users[1].Id, ResourceId = 9, EventType = EventType.Interruption, Timestamp = now.AddHours(-5) },
                new EventHistory { UserId = Users[1].Id, ResourceId = -1, EventType = EventType.Break, Timestamp = now.AddHours(-2) }
            };

            Context.AddRange(previous);
            await Context.SaveChangesAsync().ConfigureAwait(false);

            var result = await Subject.UpdateTimeRange(Users[0].Id, range).ConfigureAwait(false);
            var histories1 = await EventUnitOfWork.EventHistory.GetHistories(Users[0].Id, DateTime.MinValue, DateTime.MaxValue).ConfigureAwait(false);
            var histories2 = await EventUnitOfWork.EventHistory.GetHistories(Users[1].Id, DateTime.MinValue, DateTime.MaxValue).ConfigureAwait(false);

            Assert.IsTrue(result);
            AreEqual(expected1, histories1);
            AreEqual(expected2, histories2);
        }

        [Test]
        public async Task UpdateTimeRangeShouldMergeAllTimeRangesWhenBothEventBeforeAndAfterMatchTargetEvent()
        {
            var now = DateTime.UtcNow;
            var range = new EventTimeRangeDto { Id = 2, EventType = EventType.Task, Start = now.AddHours(-5), End = now.AddHours(-3) };

            var previous = new List<EventHistory>
            {
                new EventHistory { UserId = Users[1].Id, ResourceId = 5, EventType = EventType.Interruption, Timestamp = now.AddHours(-10) },
                new EventHistory { UserId = Users[0].Id, ResourceId = 2, EventType = EventType.Task, Timestamp = now.AddHours(-10) },
                new EventHistory { UserId = Users[1].Id, ResourceId = 7, EventType = EventType.Task, Timestamp = now.AddHours(-4) },
                new EventHistory { UserId = Users[0].Id, ResourceId = 3, EventType = EventType.Interruption, Timestamp = now.AddHours(-4) },
                new EventHistory { UserId = Users[0].Id, ResourceId = 2, EventType = EventType.Task, Timestamp = now.AddHours(-3) }
            };

            var expected1 = new List<EventHistory>
            {
                new EventHistory { UserId = Users[0].Id, ResourceId = 2, EventType = EventType.Task, Timestamp = now.AddHours(-10) }
            };

            var expected2 = new List<EventHistory>
            {
                new EventHistory { UserId = Users[1].Id, ResourceId = 5, EventType = EventType.Interruption, Timestamp = now.AddHours(-10) },
                new EventHistory { UserId = Users[1].Id, ResourceId = 7, EventType = EventType.Task, Timestamp = now.AddHours(-4) }
            };

            Context.AddRange(previous);
            await Context.SaveChangesAsync().ConfigureAwait(false);

            var result = await Subject.UpdateTimeRange(Users[0].Id, range).ConfigureAwait(false);
            var histories1 = await EventUnitOfWork.EventHistory.GetHistories(Users[0].Id, DateTime.MinValue, DateTime.MaxValue).ConfigureAwait(false);
            var histories2 = await EventUnitOfWork.EventHistory.GetHistories(Users[1].Id, DateTime.MinValue, DateTime.MaxValue).ConfigureAwait(false);

            Assert.IsTrue(result);
            AreEqual(expected1, histories1);
            AreEqual(expected2, histories2);
        }

        [Test]
        public async Task UpdateTimeRangeShouldDoNothingWhenIdlingTimeIsUnchanged()
        {
            var now = DateTime.UtcNow;
            var range = new EventTimeRangeDto { Id = -1, EventType = EventType.Idling, Start = now.AddHours(-5), End = now.AddHours(-3) };

            var previous = new List<EventHistory>
            {
                new EventHistory { UserId = Users[0].Id, ResourceId = -1, EventType = EventType.Idling, Timestamp = now.AddHours(-10) },
                new EventHistory { UserId = Users[1].Id, ResourceId = -1, EventType = EventType.Idling, Timestamp = now.AddHours(-10) },
                new EventHistory { UserId = Users[0].Id, ResourceId = 2, EventType = EventType.Task, Timestamp = now.AddHours(-3) },
                new EventHistory { UserId = Users[1].Id, ResourceId = 2, EventType = EventType.Interruption, Timestamp = now.AddHours(-3) }
            };

            var expected1 = new List<EventHistory>
            {
                new EventHistory { UserId = Users[0].Id, ResourceId = -1, EventType = EventType.Idling, Timestamp = now.AddHours(-10) },
                new EventHistory { UserId = Users[0].Id, ResourceId = 2, EventType = EventType.Task, Timestamp = now.AddHours(-3) }
            };

            var expected2 = new List<EventHistory>
            {
                new EventHistory { UserId = Users[1].Id, ResourceId = -1, EventType = EventType.Idling, Timestamp = now.AddHours(-10) },
                new EventHistory { UserId = Users[1].Id, ResourceId = 2, EventType = EventType.Interruption, Timestamp = now.AddHours(-3) }
            };

            Context.AddRange(previous);
            await Context.SaveChangesAsync().ConfigureAwait(false);

            var result = await Subject.UpdateTimeRange(Users[0].Id, range).ConfigureAwait(false);
            var histories1 = await EventUnitOfWork.EventHistory.GetHistories(Users[0].Id, DateTime.MinValue, DateTime.MaxValue).ConfigureAwait(false);
            var histories2 = await EventUnitOfWork.EventHistory.GetHistories(Users[1].Id, DateTime.MinValue, DateTime.MaxValue).ConfigureAwait(false);

            Assert.IsTrue(result);
            AreEqual(expected1, histories1);
            AreEqual(expected2, histories2);
        }

        [Test]
        public async Task UpdateTimeRangeShouldUpdateTimeRangeForSameEventWhenStartTimeIsTheSame()
        {
            var now = DateTime.UtcNow;
            var range = new EventTimeRangeDto { Id = -1, EventType = EventType.Break, Start = now.AddHours(-5), End = now.AddHours(-3) };

            var previous = new List<EventHistory>
            {
                new EventHistory { UserId = Users[0].Id, ResourceId = -1, EventType = EventType.Break, Timestamp = now.AddHours(-5) },
                new EventHistory { UserId = Users[1].Id, ResourceId = -1, EventType = EventType.Break, Timestamp = now.AddHours(-5) },
                new EventHistory { UserId = Users[0].Id, ResourceId = 3, EventType = EventType.Interruption, Timestamp = now.AddHours(-1) },
                new EventHistory { UserId = Users[1].Id, ResourceId = 3, EventType = EventType.Task, Timestamp = now.AddHours(-1) }
            };

            var expected1 = new List<EventHistory>
            {
                new EventHistory { UserId = Users[0].Id, ResourceId = -1, EventType = EventType.Break, Timestamp = now.AddHours(-5) },
                new EventHistory { UserId = Users[0].Id, ResourceId = -1, EventType = EventType.Idling, Timestamp = now.AddHours(-3) },
                new EventHistory { UserId = Users[0].Id, ResourceId = 3, EventType = EventType.Interruption, Timestamp = now.AddHours(-1) }
            };

            var expected2 = new List<EventHistory>
            {
                new EventHistory { UserId = Users[1].Id, ResourceId = -1, EventType = EventType.Break, Timestamp = now.AddHours(-5) },
                new EventHistory { UserId = Users[1].Id, ResourceId = 3, EventType = EventType.Task, Timestamp = now.AddHours(-1) }
            };

            Context.AddRange(previous);
            await Context.SaveChangesAsync().ConfigureAwait(false);

            var result = await Subject.UpdateTimeRange(Users[0].Id, range).ConfigureAwait(false);
            var histories1 = await EventUnitOfWork.EventHistory.GetHistories(Users[0].Id, DateTime.MinValue, DateTime.MaxValue).ConfigureAwait(false);
            var histories2 = await EventUnitOfWork.EventHistory.GetHistories(Users[1].Id, DateTime.MinValue, DateTime.MaxValue).ConfigureAwait(false);

            Assert.IsTrue(result);
            AreEqual(expected1, histories1);
            AreEqual(expected2, histories2);
        }

        [Test]
        public async Task UpdateTimeRangeShouldUpdateTimeRangeForSameEventWhenEndTimeIsTheSame()
        {
            var now = DateTime.UtcNow;
            var range = new EventTimeRangeDto { Id = 2, EventType = EventType.Task, Start = now.AddHours(-5), End = now.AddHours(-3) };

            var previous = new List<EventHistory>
            {
                new EventHistory { UserId = Users[0].Id, ResourceId = -1, EventType = EventType.Break, Timestamp = now.AddHours(-12) },
                new EventHistory { UserId = Users[1].Id, ResourceId = -1, EventType = EventType.Break, Timestamp = now.AddHours(-12) },
                new EventHistory { UserId = Users[0].Id, ResourceId = 2, EventType = EventType.Task, Timestamp = now.AddHours(-10) },
                new EventHistory { UserId = Users[1].Id, ResourceId = 2, EventType = EventType.Interruption, Timestamp = now.AddHours(-10) },
                new EventHistory { UserId = Users[0].Id, ResourceId = 3, EventType = EventType.Interruption, Timestamp = now.AddHours(-3) },
                new EventHistory { UserId = Users[1].Id, ResourceId = 3, EventType = EventType.Task, Timestamp = now.AddHours(-3) }
            };

            var expected1 = new List<EventHistory>
            {
                new EventHistory { UserId = Users[0].Id, ResourceId = -1, EventType = EventType.Break, Timestamp = now.AddHours(-12) },
                new EventHistory { UserId = Users[0].Id, ResourceId = -1, EventType = EventType.Idling, Timestamp = now.AddHours(-10) },
                new EventHistory { UserId = Users[0].Id, ResourceId = 2, EventType = EventType.Task, Timestamp = now.AddHours(-5) },
                new EventHistory { UserId = Users[0].Id, ResourceId = 3, EventType = EventType.Interruption, Timestamp = now.AddHours(-3) }
            };

            var expected2 = new List<EventHistory>
            {
                new EventHistory { UserId = Users[1].Id, ResourceId = -1, EventType = EventType.Break, Timestamp = now.AddHours(-12) },
                new EventHistory { UserId = Users[1].Id, ResourceId = 2, EventType = EventType.Interruption, Timestamp = now.AddHours(-10) },
                new EventHistory { UserId = Users[1].Id, ResourceId = 3, EventType = EventType.Task, Timestamp = now.AddHours(-3) }
            };

            Context.AddRange(previous);
            await Context.SaveChangesAsync().ConfigureAwait(false);

            var result = await Subject.UpdateTimeRange(Users[0].Id, range).ConfigureAwait(false);
            var histories1 = await EventUnitOfWork.EventHistory.GetHistories(Users[0].Id, DateTime.MinValue, DateTime.MaxValue).ConfigureAwait(false);
            var histories2 = await EventUnitOfWork.EventHistory.GetHistories(Users[1].Id, DateTime.MinValue, DateTime.MaxValue).ConfigureAwait(false);

            Assert.IsTrue(result);
            AreEqual(expected1, histories1);
            AreEqual(expected2, histories2);
        }

        [Test]
        public async Task UpdateTimeRangeShouldUpdateTimeRangeForSameEventWhenNeitherStartTimeNorEndTimeIsTheSame()
        {
            var now = DateTime.UtcNow;
            var range = new EventTimeRangeDto { Id = 5, EventType = EventType.Interruption, Start = now.AddHours(-5), End = now.AddHours(-3) };

            var previous = new List<EventHistory>
            {
                new EventHistory { UserId = Users[0].Id, ResourceId = -1, EventType = EventType.Idling, Timestamp = now.AddHours(-12) },
                new EventHistory { UserId = Users[1].Id, ResourceId = -1, EventType = EventType.Idling, Timestamp = now.AddHours(-12) },
                new EventHistory { UserId = Users[0].Id, ResourceId = 5, EventType = EventType.Interruption, Timestamp = now.AddHours(-10) },
                new EventHistory { UserId = Users[1].Id, ResourceId = 5, EventType = EventType.Task, Timestamp = now.AddHours(-10) },
                new EventHistory { UserId = Users[0].Id, ResourceId = 9, EventType = EventType.Task, Timestamp = now.AddHours(-1) },
                new EventHistory { UserId = Users[1].Id, ResourceId = 9, EventType = EventType.Interruption, Timestamp = now.AddHours(-1) }
            };

            var expected1 = new List<EventHistory>
            {
                new EventHistory { UserId = Users[0].Id, ResourceId = -1, EventType = EventType.Idling, Timestamp = now.AddHours(-12) },
                new EventHistory { UserId = Users[0].Id, ResourceId = 5, EventType = EventType.Interruption, Timestamp = now.AddHours(-5) },
                new EventHistory { UserId = Users[0].Id, ResourceId = -1, EventType = EventType.Idling, Timestamp = now.AddHours(-3) },
                new EventHistory { UserId = Users[0].Id, ResourceId = 9, EventType = EventType.Task, Timestamp = now.AddHours(-1) }
            };

            var expected2 = new List<EventHistory>
            {
                new EventHistory { UserId = Users[1].Id, ResourceId = -1, EventType = EventType.Idling, Timestamp = now.AddHours(-12) },
                new EventHistory { UserId = Users[1].Id, ResourceId = 5, EventType = EventType.Task, Timestamp = now.AddHours(-10) },
                new EventHistory { UserId = Users[1].Id, ResourceId = 9, EventType = EventType.Interruption, Timestamp = now.AddHours(-1) }
            };

            Context.AddRange(previous);
            await Context.SaveChangesAsync().ConfigureAwait(false);

            var result = await Subject.UpdateTimeRange(Users[0].Id, range).ConfigureAwait(false);
            var histories1 = await EventUnitOfWork.EventHistory.GetHistories(Users[0].Id, DateTime.MinValue, DateTime.MaxValue).ConfigureAwait(false);
            var histories2 = await EventUnitOfWork.EventHistory.GetHistories(Users[1].Id, DateTime.MinValue, DateTime.MaxValue).ConfigureAwait(false);

            Assert.IsTrue(result);
            AreEqual(expected1, histories1);
            AreEqual(expected2, histories2);
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
