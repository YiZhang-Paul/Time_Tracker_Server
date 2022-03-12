using Core.Enums;
using Core.Interfaces.Repositories;
using Core.Models.Event;
using Moq;
using NUnit.Framework;
using Service.Services;
using System;
using System.Threading.Tasks;

namespace Service.Test.Unit.Services
{
    [TestFixture]
    public class EventTrackingServiceTest
    {
        private Mock<IEventHistoryRepository> EventHistoryRepository { get; set; }
        private Mock<IEventPromptRepository> EventPromptRepository { get; set; }
        private EventTrackingService Subject { get; set; }

        [SetUp]
        public void Setup()
        {
            EventHistoryRepository = new Mock<IEventHistoryRepository>();
            EventPromptRepository = new Mock<IEventPromptRepository>();
            Subject = new EventTrackingService(EventHistoryRepository.Object, EventPromptRepository.Object);
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
        public async Task StartInterruptionItemShouldReturnFalseWhenItemIsOngoing()
        {
            var history = new EventHistory { ResourceId = 5, EventType = EventType.Interruption };
            EventHistoryRepository.Setup(_ => _.GetLastHistory(It.IsAny<DateTime?>(), It.IsAny<bool>())).ReturnsAsync(history);

            var result = await Subject.StartInterruptionItem(5).ConfigureAwait(false);

            Assert.IsFalse(result);
            EventHistoryRepository.Verify(_ => _.GetLastHistory(null, It.IsAny<bool>()), Times.Once);
            EventHistoryRepository.Verify(_ => _.CreateHistory(It.IsAny<EventHistory>()), Times.Never);
        }

        [Test]
        public async Task StartInterruptionItemShouldReturnFalseWhenFailedToStartItem()
        {
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
        public async Task StartTaskItemShouldReturnFalseWhenItemIsOngoing()
        {
            var history = new EventHistory { ResourceId = 5, EventType = EventType.Task };
            EventHistoryRepository.Setup(_ => _.GetLastHistory(It.IsAny<DateTime?>(), It.IsAny<bool>())).ReturnsAsync(history);

            var result = await Subject.StartTaskItem(5).ConfigureAwait(false);

            Assert.IsFalse(result);
            EventHistoryRepository.Verify(_ => _.GetLastHistory(null, It.IsAny<bool>()), Times.Once);
            EventHistoryRepository.Verify(_ => _.CreateHistory(It.IsAny<EventHistory>()), Times.Never);
        }

        [Test]
        public async Task StartTaskItemShouldReturnFalseWhenFailedToStartItem()
        {
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
            Assert.ThrowsAsync<ArgumentException>(async () => await Subject.StartBreakSession(duration).ConfigureAwait(false));
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
