using Core.Enums;
using Core.Interfaces.Repositories;
using Core.Models.Event;
using Core.Models.Interruption;
using Core.Models.Task;
using Moq;
using NUnit.Framework;
using Service.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Service.Test.Unit.Services
{
    [TestFixture]
    public class EventServiceTest
    {
        private Mock<IInterruptionItemRepository> InterruptionItemRepository { get; set; }
        private Mock<ITaskItemRepository> TaskItemRepository { get; set; }
        private Mock<IEventHistoryRepository> EventHistoryRepository { get; set; }
        private Mock<IEventHistorySummaryRepository> EventHistorySummaryRepository { get; set; }
        private Mock<IEventPromptRepository> EventPromptRepository { get; set; }
        private EventService Subject { get; set; }

        [SetUp]
        public void Setup()
        {
            InterruptionItemRepository = new Mock<IInterruptionItemRepository>();
            TaskItemRepository = new Mock<ITaskItemRepository>();
            EventHistoryRepository = new Mock<IEventHistoryRepository>();
            EventHistorySummaryRepository = new Mock<IEventHistorySummaryRepository>();
            EventPromptRepository = new Mock<IEventPromptRepository>();

            Subject = new EventService
            (
                InterruptionItemRepository.Object,
                TaskItemRepository.Object,
                EventHistoryRepository.Object,
                EventHistorySummaryRepository.Object,
                EventPromptRepository.Object
            );
        }

        [Test]
        public async Task GetOngoingTimeSummaryShouldGetTimeSummaryWithinProperTimeRange()
        {
            var end = DateTime.UtcNow;
            var start = end.Date;
            var promptTime = end.AddMinutes(-15);
            EventPromptRepository.Setup(_ => _.GetLastPrompt(It.IsAny<PromptType>())).ReturnsAsync(new EventPrompt { Timestamp = promptTime });
            EventHistoryRepository.Setup(_ => _.GetHistories(It.IsAny<DateTime>(), It.IsAny<DateTime>())).ReturnsAsync(new List<EventHistory>());

            await Subject.GetOngoingTimeSummary(start).ConfigureAwait(false);

            EventHistoryRepository.Verify(_ => _.GetHistories(start, It.Is<DateTime>(time => (time - end).Duration().TotalMilliseconds < 100)), Times.Once);
            EventHistoryRepository.Verify(_ => _.GetHistories(promptTime, It.Is<DateTime>(time => (time - end).Duration().TotalMilliseconds < 100)), Times.Once);
            EventHistoryRepository.Verify(_ => _.GetLastHistory(null, true), Times.Exactly(2));
        }

        [Test]
        public async Task GetOngoingTimeSummaryShouldDefaultLastPromptTimeToStartTimeWhenNoEventPromptExists()
        {
            var end = DateTime.UtcNow;
            var start = end.Date;
            EventPromptRepository.Setup(_ => _.GetLastPrompt(It.IsAny<PromptType>())).ReturnsAsync((EventPrompt)null);
            EventHistoryRepository.Setup(_ => _.GetHistories(It.IsAny<DateTime>(), It.IsAny<DateTime>())).ReturnsAsync(new List<EventHistory>());

            await Subject.GetOngoingTimeSummary(start).ConfigureAwait(false);

            EventHistoryRepository.Verify(_ => _.GetHistories(start, It.Is<DateTime>(time => (time - end).Duration().TotalMilliseconds < 500)), Times.Exactly(2));
            EventHistoryRepository.Verify(_ => _.GetLastHistory(null, true), Times.Exactly(2));
        }

        [Test]
        public async Task GetOngoingTimeSummaryShouldReturnEmptyConcludedTimeWhenNoEventHistoryAvailable()
        {
            EventHistoryRepository.Setup(_ => _.GetHistories(It.IsAny<DateTime>(), It.IsAny<DateTime>())).ReturnsAsync(new List<EventHistory>());

            var result = await Subject.GetOngoingTimeSummary(DateTime.UtcNow.Date).ConfigureAwait(false);

            Assert.AreEqual(0, result.ConcludedSinceStart.Working);
            Assert.AreEqual(0, result.ConcludedSinceStart.NotWorking);
            Assert.AreEqual(0, result.ConcludedSinceLastBreakPrompt.Working);
            Assert.AreEqual(0, result.ConcludedSinceLastBreakPrompt.NotWorking);
        }

        [Test]
        public async Task GetOngoingTimeSummaryShouldIncludeConcludedTimeWithinSpecifiedTimeRange()
        {
            var now = DateTime.UtcNow;
            var start = now.AddMinutes(-30);
            // 15 minutes idling before the break
            var histories = new List<EventHistory> { new EventHistory { EventType = EventType.Break, Timestamp = now.AddMinutes(-15) } };
            EventHistoryRepository.Setup(_ => _.GetHistories(It.IsAny<DateTime>(), It.IsAny<DateTime>())).ReturnsAsync(histories);
            EventHistoryRepository.Setup(_ => _.GetHistoryById(It.IsAny<long>())).ReturnsAsync((EventHistory)null);

            var result = await Subject.GetOngoingTimeSummary(start).ConfigureAwait(false);

            Assert.AreEqual(0, result.ConcludedSinceStart.Working);
            Assert.AreEqual(15 * 60000, result.ConcludedSinceStart.NotWorking, 1);
        }

        [Test]
        public async Task GetOngoingTimeSummaryShouldReturnConcludedTime()
        {
            var now = DateTime.UtcNow;
            var start = now.AddMinutes(-30);
            var promptTime = now.AddMinutes(-8);

            var historiesSinceStart = new List<EventHistory>
            {
                new EventHistory { EventType = EventType.Break, Timestamp = now.AddMinutes(-15) }, // break for 3 minutes, with 15 minutes interruption before the break
                new EventHistory { EventType = EventType.Idling, Timestamp = now.AddMinutes(-12) }, // idle for 2 minutes
                new EventHistory { EventType = EventType.Interruption, Timestamp = now.AddMinutes(-10) }, // interruption for 4 minutes
                new EventHistory { EventType = EventType.Task, Timestamp = now.AddMinutes(-6) }, // task for 3 minutes
                new EventHistory { EventType = EventType.Idling, Timestamp = now.AddMinutes(-3) }
            };

            var historiesSincePrompt = new List<EventHistory>
            {
                new EventHistory { EventType = EventType.Interruption, Timestamp = now.AddMinutes(-8) }, // interruption for 2 minutes
                new EventHistory { EventType = EventType.Task, Timestamp = now.AddMinutes(-6) }, // task for 3 minutes
                new EventHistory { EventType = EventType.Idling, Timestamp = now.AddMinutes(-3) }
            };

            EventHistoryRepository.Setup(_ => _.GetHistories(start, It.IsAny<DateTime>())).ReturnsAsync(historiesSinceStart);
            EventHistoryRepository.Setup(_ => _.GetHistories(promptTime, It.IsAny<DateTime>())).ReturnsAsync(historiesSincePrompt);
            EventHistoryRepository.Setup(_ => _.GetHistoryById(It.IsAny<long>())).ReturnsAsync(new EventHistory { EventType = EventType.Interruption, Timestamp = start.AddMinutes(-30) });
            EventPromptRepository.Setup(_ => _.GetLastPrompt(It.IsAny<PromptType>())).ReturnsAsync(new EventPrompt { Timestamp = promptTime });

            var result = await Subject.GetOngoingTimeSummary(start).ConfigureAwait(false);

            Assert.AreEqual(22 * 60000, result.ConcludedSinceStart.Working);
            Assert.AreEqual(5 * 60000, result.ConcludedSinceStart.NotWorking, 1);
            Assert.AreEqual(5 * 60000, result.ConcludedSinceLastBreakPrompt.Working);
            Assert.AreEqual(0, result.ConcludedSinceLastBreakPrompt.NotWorking);
        }

        [Test]
        public async Task GetOngoingTimeSummaryShouldDefaultToIdlingTimeWhenNoEventHistoryExists()
        {
            var now = DateTime.UtcNow;
            EventHistoryRepository.Setup(_ => _.GetHistories(It.IsAny<DateTime>(), It.IsAny<DateTime>())).ReturnsAsync(new List<EventHistory>());
            EventPromptRepository.Setup(_ => _.GetLastPrompt(It.IsAny<PromptType>())).ReturnsAsync(new EventPrompt { Timestamp = now.AddMinutes(-10) });
            EventHistoryRepository.Setup(_ => _.GetLastHistory(It.IsAny<DateTime?>(), It.IsAny<bool>())).ReturnsAsync((EventHistory)null);

            var result = await Subject.GetOngoingTimeSummary(now.AddMinutes(-60)).ConfigureAwait(false);

            Assert.AreEqual(-1, result.UnconcludedSinceStart.Id);
            Assert.AreEqual(-1, result.UnconcludedSinceStart.ResourceId);
            Assert.AreEqual(EventType.Idling, result.UnconcludedSinceStart.EventType);
            Assert.AreEqual(now.AddMinutes(-60), result.UnconcludedSinceStart.Timestamp);
            Assert.AreEqual(-1, result.UnconcludedSinceLastBreakPrompt.Id);
            Assert.AreEqual(-1, result.UnconcludedSinceLastBreakPrompt.ResourceId);
            Assert.AreEqual(EventType.Idling, result.UnconcludedSinceLastBreakPrompt.EventType);
            Assert.AreEqual(now.AddMinutes(-10), result.UnconcludedSinceLastBreakPrompt.Timestamp);
        }

        [Test]
        public async Task GetOngoingTimeSummaryShouldReturnUnconcludedTime()
        {
            var now = DateTime.UtcNow;
            EventHistoryRepository.Setup(_ => _.GetHistories(It.IsAny<DateTime>(), It.IsAny<DateTime>())).ReturnsAsync(new List<EventHistory>());
            EventPromptRepository.Setup(_ => _.GetLastPrompt(It.IsAny<PromptType>())).ReturnsAsync(new EventPrompt { Timestamp = now.AddMinutes(-10) });

            EventHistoryRepository.SetupSequence(_ => _.GetLastHistory(It.IsAny<DateTime?>(), It.IsAny<bool>()))
                .ReturnsAsync(new EventHistory { Timestamp = DateTime.SpecifyKind(now.AddMinutes(-30), DateTimeKind.Unspecified) })
                .ReturnsAsync(new EventHistory { Timestamp = DateTime.SpecifyKind(now.AddMinutes(-30), DateTimeKind.Unspecified) });

            var result = await Subject.GetOngoingTimeSummary(now.AddMinutes(-60)).ConfigureAwait(false);

            Assert.AreEqual(now.AddMinutes(-30), result.UnconcludedSinceStart.Timestamp);
            Assert.AreEqual(now.AddMinutes(-10), result.UnconcludedSinceLastBreakPrompt.Timestamp);
            Assert.AreEqual(DateTimeKind.Utc, result.UnconcludedSinceStart.Timestamp.Kind);
            Assert.AreEqual(DateTimeKind.Utc, result.UnconcludedSinceLastBreakPrompt.Timestamp.Kind);
        }

        [Test]
        public async Task StartIdlingSessionShouldReturnFalseWhenIdlingSessionIsOngoing()
        {
            var history = new EventHistory { EventType = EventType.Idling };
            EventHistoryRepository.Setup(_ => _.GetLastHistory(It.IsAny<DateTime?>(), It.IsAny<bool>())).ReturnsAsync(history);

            var result = await Subject.StartIdlingSession().ConfigureAwait(false);

            Assert.IsFalse(result);
            EventHistoryRepository.Verify(_ => _.GetLastHistory(null, It.IsAny<bool>()), Times.Once);
            EventHistoryRepository.Verify(_ => _.CreateHistory(It.IsAny<EventHistory>()), Times.Never);
        }

        [Test]
        public async Task StartIdlingSessionShouldReturnFalseWhenFailedToStartSession()
        {
            EventHistoryRepository.Setup(_ => _.GetLastHistory(It.IsAny<DateTime?>(), It.IsAny<bool>())).ReturnsAsync((EventHistory)null);
            EventHistoryRepository.Setup(_ => _.CreateHistory(It.IsAny<EventHistory>())).ReturnsAsync((EventHistory)null);

            var result = await Subject.StartIdlingSession().ConfigureAwait(false);

            Assert.IsFalse(result);

            EventHistoryRepository.Verify(_ => _.CreateHistory(It.Is<EventHistory>
            (
                history => history.ResourceId == -1 && history.EventType == EventType.Idling
            )), Times.Once);
        }

        [Test]
        public async Task StartIdlingSessionShouldReturnTrueWhenSuccessfullyStartedSession()
        {
            var history = new EventHistory { EventType = EventType.Task };
            EventHistoryRepository.Setup(_ => _.GetLastHistory(It.IsAny<DateTime?>(), It.IsAny<bool>())).ReturnsAsync(history);
            EventHistoryRepository.Setup(_ => _.CreateHistory(It.IsAny<EventHistory>())).ReturnsAsync(new EventHistory());

            var result = await Subject.StartIdlingSession().ConfigureAwait(false);

            Assert.IsTrue(result);

            EventHistoryRepository.Verify(_ => _.CreateHistory(It.Is<EventHistory>
            (
                history => history.ResourceId == -1 && history.EventType == EventType.Idling
            )), Times.Once);
        }

        [Test]
        public async Task StartInterruptionItemShouldReturnFalseWhenItemDoesNotExist()
        {
            InterruptionItemRepository.Setup(_ => _.GetItemById(It.IsAny<long>(), true)).ReturnsAsync((InterruptionItem)null);

            var result = await Subject.StartInterruptionItem(5).ConfigureAwait(false);

            Assert.IsFalse(result);
            InterruptionItemRepository.Verify(_ => _.GetItemById(It.IsAny<long>(), true), Times.Once);
            EventHistoryRepository.Verify(_ => _.CreateHistory(It.IsAny<EventHistory>()), Times.Never);
        }

        [Test]
        public async Task StartInterruptionItemShouldReturnFalseWhenItemIsOngoing()
        {
            var history = new EventHistory { ResourceId = 5, EventType = EventType.Interruption };
            InterruptionItemRepository.Setup(_ => _.GetItemById(It.IsAny<long>(), true)).ReturnsAsync(new InterruptionItem());
            EventHistoryRepository.Setup(_ => _.GetLastHistory(It.IsAny<DateTime?>(), It.IsAny<bool>())).ReturnsAsync(history);

            var result = await Subject.StartInterruptionItem(5).ConfigureAwait(false);

            Assert.IsFalse(result);
            EventHistoryRepository.Verify(_ => _.GetLastHistory(null, It.IsAny<bool>()), Times.Once);
            EventHistoryRepository.Verify(_ => _.CreateHistory(It.IsAny<EventHistory>()), Times.Never);
        }

        [Test]
        public async Task StartInterruptionItemShouldReturnFalseWhenFailedToStartItem()
        {
            InterruptionItemRepository.Setup(_ => _.GetItemById(It.IsAny<long>(), true)).ReturnsAsync(new InterruptionItem());
            EventHistoryRepository.Setup(_ => _.GetLastHistory(It.IsAny<DateTime?>(), It.IsAny<bool>())).ReturnsAsync((EventHistory)null);
            EventHistoryRepository.Setup(_ => _.CreateHistory(It.IsAny<EventHistory>())).ReturnsAsync((EventHistory)null);

            var result = await Subject.StartInterruptionItem(5).ConfigureAwait(false);

            Assert.IsFalse(result);

            EventHistoryRepository.Verify(_ => _.CreateHistory(It.Is<EventHistory>
            (
                history => history.ResourceId == 5 && history.EventType == EventType.Interruption
            )), Times.Once);
        }

        [Test]
        public async Task StartInterruptionItemShouldReturnTrueWhenSuccessfullyStartedItem()
        {
            var history = new EventHistory { ResourceId = 6, EventType = EventType.Interruption };
            InterruptionItemRepository.Setup(_ => _.GetItemById(It.IsAny<long>(), true)).ReturnsAsync(new InterruptionItem());
            EventHistoryRepository.Setup(_ => _.GetLastHistory(It.IsAny<DateTime?>(), It.IsAny<bool>())).ReturnsAsync(history);
            EventHistoryRepository.Setup(_ => _.CreateHistory(It.IsAny<EventHistory>())).ReturnsAsync(new EventHistory());

            var result = await Subject.StartInterruptionItem(5).ConfigureAwait(false);

            Assert.IsTrue(result);

            EventHistoryRepository.Verify(_ => _.CreateHistory(It.Is<EventHistory>
            (
                history => history.ResourceId == 5 && history.EventType == EventType.Interruption
            )), Times.Once);
        }

        [Test]
        public async Task StartTaskItemShouldReturnFalseWhenItemDoesNotExist()
        {
            TaskItemRepository.Setup(_ => _.GetItemById(It.IsAny<long>(), true)).ReturnsAsync((TaskItem)null);

            var result = await Subject.StartTaskItem(5).ConfigureAwait(false);

            Assert.IsFalse(result);
            TaskItemRepository.Verify(_ => _.GetItemById(It.IsAny<long>(), true), Times.Once);
            EventHistoryRepository.Verify(_ => _.CreateHistory(It.IsAny<EventHistory>()), Times.Never);
        }

        [Test]
        public async Task StartTaskItemShouldReturnFalseWhenItemIsOngoing()
        {
            var history = new EventHistory { ResourceId = 5, EventType = EventType.Task };
            TaskItemRepository.Setup(_ => _.GetItemById(It.IsAny<long>(), true)).ReturnsAsync(new TaskItem());
            EventHistoryRepository.Setup(_ => _.GetLastHistory(It.IsAny<DateTime?>(), It.IsAny<bool>())).ReturnsAsync(history);

            var result = await Subject.StartTaskItem(5).ConfigureAwait(false);

            Assert.IsFalse(result);
            EventHistoryRepository.Verify(_ => _.GetLastHistory(null, It.IsAny<bool>()), Times.Once);
            EventHistoryRepository.Verify(_ => _.CreateHistory(It.IsAny<EventHistory>()), Times.Never);
        }

        [Test]
        public async Task StartTaskItemShouldReturnFalseWhenFailedToStartItem()
        {
            TaskItemRepository.Setup(_ => _.GetItemById(It.IsAny<long>(), true)).ReturnsAsync(new TaskItem());
            EventHistoryRepository.Setup(_ => _.GetLastHistory(It.IsAny<DateTime?>(), It.IsAny<bool>())).ReturnsAsync((EventHistory)null);
            EventHistoryRepository.Setup(_ => _.CreateHistory(It.IsAny<EventHistory>())).ReturnsAsync((EventHistory)null);

            var result = await Subject.StartTaskItem(5).ConfigureAwait(false);

            Assert.IsFalse(result);

            EventHistoryRepository.Verify(_ => _.CreateHistory(It.Is<EventHistory>
            (
                history => history.ResourceId == 5 && history.EventType == EventType.Task
            )), Times.Once);
        }

        [Test]
        public async Task StartTaskItemShouldReturnTrueWhenSuccessfullyStartedItem()
        {
            var history = new EventHistory { ResourceId = 6, EventType = EventType.Task };
            TaskItemRepository.Setup(_ => _.GetItemById(It.IsAny<long>(), true)).ReturnsAsync(new TaskItem());
            EventHistoryRepository.Setup(_ => _.GetLastHistory(It.IsAny<DateTime?>(), It.IsAny<bool>())).ReturnsAsync(history);
            EventHistoryRepository.Setup(_ => _.CreateHistory(It.IsAny<EventHistory>())).ReturnsAsync(new EventHistory());

            var result = await Subject.StartTaskItem(5).ConfigureAwait(false);

            Assert.IsTrue(result);

            EventHistoryRepository.Verify(_ => _.CreateHistory(It.Is<EventHistory>
            (
                history => history.ResourceId == 5 && history.EventType == EventType.Task
            )), Times.Once);
        }

        [Test]
        [TestCase(-1)]
        [TestCase(0)]
        [TestCase(299999)]
        public void StartBreakSessionShouldThrowWhenTargetDurationIsInvalid(int duration)
        {
            Assert.ThrowsAsync<ArgumentException>(async() => await Subject.StartBreakSession(duration).ConfigureAwait(false));
        }

        [Test]
        public async Task StartBreakSessionShouldReturnFalseWhenFailedToRecordEventPrompt()
        {
            EventPromptRepository.Setup(_ => _.CreatePrompt(It.IsAny<EventPrompt>())).ReturnsAsync((EventPrompt)null);

            var result = await Subject.StartBreakSession(300000).ConfigureAwait(false);

            Assert.IsFalse(result);
            EventHistoryRepository.Verify(_ => _.CreateHistory(It.IsAny<EventHistory>()), Times.Never);

            EventPromptRepository.Verify(_ => _.CreatePrompt(It.Is<EventPrompt>
            (
                prompt => prompt.PromptType == PromptType.ScheduledBreak && prompt.ConfirmType == PromptConfirmType.Commenced
            )), Times.Once);
        }

        [Test]
        public async Task StartBreakSessionShouldReturnFalseWhenBreakSessionIsOngoing()
        {
            var history = new EventHistory { EventType = EventType.Break };
            EventPromptRepository.Setup(_ => _.CreatePrompt(It.IsAny<EventPrompt>())).ReturnsAsync(new EventPrompt());
            EventHistoryRepository.Setup(_ => _.GetLastHistory(It.IsAny<DateTime?>(), It.IsAny<bool>())).ReturnsAsync(history);

            var result = await Subject.StartBreakSession(300000).ConfigureAwait(false);

            Assert.IsFalse(result);
            EventHistoryRepository.Verify(_ => _.GetLastHistory(null, It.IsAny<bool>()), Times.Once);
            EventHistoryRepository.Verify(_ => _.CreateHistory(It.IsAny<EventHistory>()), Times.Never);
        }

        [Test]
        public async Task StartBreakSessionShouldReturnFalseWhenFailedToStartSession()
        {
            EventPromptRepository.Setup(_ => _.CreatePrompt(It.IsAny<EventPrompt>())).ReturnsAsync(new EventPrompt());
            EventHistoryRepository.Setup(_ => _.GetLastHistory(It.IsAny<DateTime?>(), It.IsAny<bool>())).ReturnsAsync((EventHistory)null);
            EventHistoryRepository.Setup(_ => _.CreateHistory(It.IsAny<EventHistory>())).ReturnsAsync((EventHistory)null);

            var result = await Subject.StartBreakSession(300000).ConfigureAwait(false);

            Assert.IsFalse(result);

            EventHistoryRepository.Verify(_ => _.CreateHistory(It.Is<EventHistory>
            (
                history => history.ResourceId == -1 &&
                           history.EventType == EventType.Break &&
                           history.TargetDuration == 300000
            )), Times.Once);
        }

        [Test]
        public async Task StartBreakSessionShouldReturnTrueWhenSuccessfullyStartedSession()
        {
            var history = new EventHistory { EventType = EventType.Interruption };
            EventPromptRepository.Setup(_ => _.CreatePrompt(It.IsAny<EventPrompt>())).ReturnsAsync(new EventPrompt());
            EventHistoryRepository.Setup(_ => _.GetLastHistory(It.IsAny<DateTime?>(), It.IsAny<bool>())).ReturnsAsync(history);
            EventHistoryRepository.Setup(_ => _.CreateHistory(It.IsAny<EventHistory>())).ReturnsAsync(new EventHistory());

            var result = await Subject.StartBreakSession(300000).ConfigureAwait(false);

            Assert.IsTrue(result);

            EventHistoryRepository.Verify(_ => _.CreateHistory(It.Is<EventHistory>
            (
                history => history.ResourceId == -1 &&
                           history.EventType == EventType.Break &&
                           history.TargetDuration == 300000
            )), Times.Once);
        }

        [Test]
        public async Task SkipBreakSessionShouldReturnFalseWhenFailedToSkipSession()
        {
            EventPromptRepository.Setup(_ => _.CreatePrompt(It.IsAny<EventPrompt>())).ReturnsAsync((EventPrompt)null);

            var result = await Subject.SkipBreakSession().ConfigureAwait(false);

            Assert.IsFalse(result);

            EventPromptRepository.Verify(_ => _.CreatePrompt(It.Is<EventPrompt>
            (
                prompt => prompt.PromptType == PromptType.ScheduledBreak && prompt.ConfirmType == PromptConfirmType.Skipped
            )), Times.Once);
        }

        [Test]
        public async Task SkipBreakSessionShouldReturnTrueWhenSuccessfullySkippedSession()
        {
            EventPromptRepository.Setup(_ => _.CreatePrompt(It.IsAny<EventPrompt>())).ReturnsAsync(new EventPrompt());

            var result = await Subject.SkipBreakSession().ConfigureAwait(false);

            Assert.IsTrue(result);

            EventPromptRepository.Verify(_ => _.CreatePrompt(It.Is<EventPrompt>
            (
                prompt => prompt.PromptType == PromptType.ScheduledBreak && prompt.ConfirmType == PromptConfirmType.Skipped
            )), Times.Once);
        }
    }
}
