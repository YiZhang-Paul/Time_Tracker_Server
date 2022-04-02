using Core.Enums;
using Core.Interfaces.Repositories;
using Core.Interfaces.UnitOfWorks;
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
        private Mock<IEventUnitOfWork> EventUnitOfWork { get; set; }
        private EventTrackingService Subject { get; set; }

        [SetUp]
        public void Setup()
        {
            EventHistoryRepository = new Mock<IEventHistoryRepository>();
            EventPromptRepository = new Mock<IEventPromptRepository>();
            EventUnitOfWork = new Mock<IEventUnitOfWork>();
            EventUnitOfWork.SetupGet(_ => _.EventHistory).Returns(EventHistoryRepository.Object);
            EventUnitOfWork.SetupGet(_ => _.EventPrompt).Returns(EventPromptRepository.Object);
            Subject = new EventTrackingService(EventUnitOfWork.Object);
        }

        [Test]
        public async Task StartIdlingSessionShouldReturnFalseWhenIdlingSessionIsOngoing()
        {
            var history = new EventHistory { EventType = EventType.Idling };
            EventHistoryRepository.Setup(_ => _.GetLastHistory(It.IsAny<long>(), It.IsAny<DateTime?>(), It.IsAny<bool>())).ReturnsAsync(history);

            var result = await Subject.StartIdlingSession(99).ConfigureAwait(false);

            Assert.IsFalse(result);
            EventHistoryRepository.Verify(_ => _.GetLastHistory(99, null, It.IsAny<bool>()), Times.Once);
            EventHistoryRepository.Verify(_ => _.CreateHistory(It.IsAny<long>(), It.IsAny<EventHistory>()), Times.Never);
            EventUnitOfWork.Verify(_ => _.Save(), Times.Never);
        }

        [Test]
        public async Task StartIdlingSessionShouldReturnFalseWhenFailedToStartSession()
        {
            EventHistoryRepository.Setup(_ => _.GetLastHistory(It.IsAny<long>(), It.IsAny<DateTime?>(), It.IsAny<bool>())).ReturnsAsync((EventHistory)null);
            EventUnitOfWork.Setup(_ => _.Save()).ReturnsAsync(false);

            var result = await Subject.StartIdlingSession(99).ConfigureAwait(false);

            Assert.IsFalse(result);
            EventUnitOfWork.Verify(_ => _.Save(), Times.Once);

            EventHistoryRepository.Verify(_ => _.CreateHistory(99, It.Is<EventHistory>
            (
                history => history.ResourceId == -1 && history.EventType == EventType.Idling
            )), Times.Once);
        }

        [Test]
        public async Task StartIdlingSessionShouldReturnTrueWhenSuccessfullyStartedSession()
        {
            var history = new EventHistory { EventType = EventType.Task };
            EventHistoryRepository.Setup(_ => _.GetLastHistory(It.IsAny<long>(), It.IsAny<DateTime?>(), It.IsAny<bool>())).ReturnsAsync(history);
            EventUnitOfWork.Setup(_ => _.Save()).ReturnsAsync(true);

            var result = await Subject.StartIdlingSession(99).ConfigureAwait(false);

            Assert.IsTrue(result);
            EventUnitOfWork.Verify(_ => _.Save(), Times.Once);

            EventHistoryRepository.Verify(_ => _.CreateHistory(99, It.Is<EventHistory>
            (
                history => history.ResourceId == -1 && history.EventType == EventType.Idling
            )), Times.Once);
        }

        [Test]
        public async Task StartInterruptionItemShouldReturnFalseWhenItemIsOngoing()
        {
            var history = new EventHistory { ResourceId = 5, EventType = EventType.Interruption };
            EventHistoryRepository.Setup(_ => _.GetLastHistory(It.IsAny<long>(), It.IsAny<DateTime?>(), It.IsAny<bool>())).ReturnsAsync(history);

            var result = await Subject.StartInterruptionItem(99, 5).ConfigureAwait(false);

            Assert.IsFalse(result);
            EventHistoryRepository.Verify(_ => _.GetLastHistory(99, null, It.IsAny<bool>()), Times.Once);
            EventHistoryRepository.Verify(_ => _.CreateHistory(It.IsAny<long>(), It.IsAny<EventHistory>()), Times.Never);
            EventUnitOfWork.Verify(_ => _.Save(), Times.Never);
        }

        [Test]
        public async Task StartInterruptionItemShouldReturnFalseWhenFailedToStartItem()
        {
            EventHistoryRepository.Setup(_ => _.GetLastHistory(It.IsAny<long>(), It.IsAny<DateTime?>(), It.IsAny<bool>())).ReturnsAsync((EventHistory)null);
            EventUnitOfWork.Setup(_ => _.Save()).ReturnsAsync(false);

            var result = await Subject.StartInterruptionItem(99, 5).ConfigureAwait(false);

            Assert.IsFalse(result);
            EventUnitOfWork.Verify(_ => _.Save(), Times.Once);

            EventHistoryRepository.Verify(_ => _.CreateHistory(99, It.Is<EventHistory>
            (
                history => history.ResourceId == 5 && history.EventType == EventType.Interruption
            )), Times.Once);
        }

        [Test]
        public async Task StartInterruptionItemShouldReturnTrueWhenSuccessfullyStartedItem()
        {
            var history = new EventHistory { ResourceId = 6, EventType = EventType.Interruption };
            EventHistoryRepository.Setup(_ => _.GetLastHistory(It.IsAny<long>(), It.IsAny<DateTime?>(), It.IsAny<bool>())).ReturnsAsync(history);
            EventUnitOfWork.Setup(_ => _.Save()).ReturnsAsync(true);

            var result = await Subject.StartInterruptionItem(99, 5).ConfigureAwait(false);

            Assert.IsTrue(result);
            EventUnitOfWork.Verify(_ => _.Save(), Times.Once);

            EventHistoryRepository.Verify(_ => _.CreateHistory(99, It.Is<EventHistory>
            (
                history => history.ResourceId == 5 && history.EventType == EventType.Interruption
            )), Times.Once);
        }

        [Test]
        public async Task StartTaskItemShouldReturnFalseWhenItemIsOngoing()
        {
            var history = new EventHistory { ResourceId = 5, EventType = EventType.Task };
            EventHistoryRepository.Setup(_ => _.GetLastHistory(It.IsAny<long>(), It.IsAny<DateTime?>(), It.IsAny<bool>())).ReturnsAsync(history);

            var result = await Subject.StartTaskItem(99, 5).ConfigureAwait(false);

            Assert.IsFalse(result);
            EventHistoryRepository.Verify(_ => _.GetLastHistory(99, null, It.IsAny<bool>()), Times.Once);
            EventHistoryRepository.Verify(_ => _.CreateHistory(It.IsAny<long>(), It.IsAny<EventHistory>()), Times.Never);
            EventUnitOfWork.Verify(_ => _.Save(), Times.Never);
        }

        [Test]
        public async Task StartTaskItemShouldReturnFalseWhenFailedToStartItem()
        {
            EventHistoryRepository.Setup(_ => _.GetLastHistory(It.IsAny<long>(), It.IsAny<DateTime?>(), It.IsAny<bool>())).ReturnsAsync((EventHistory)null);
            EventUnitOfWork.Setup(_ => _.Save()).ReturnsAsync(false);

            var result = await Subject.StartTaskItem(99, 5).ConfigureAwait(false);

            Assert.IsFalse(result);
            EventUnitOfWork.Verify(_ => _.Save(), Times.Once);

            EventHistoryRepository.Verify(_ => _.CreateHistory(99, It.Is<EventHistory>
            (
                history => history.ResourceId == 5 && history.EventType == EventType.Task
            )), Times.Once);
        }

        [Test]
        public async Task StartTaskItemShouldReturnTrueWhenSuccessfullyStartedItem()
        {
            var history = new EventHistory { ResourceId = 6, EventType = EventType.Task };
            EventHistoryRepository.Setup(_ => _.GetLastHistory(It.IsAny<long>(), It.IsAny<DateTime?>(), It.IsAny<bool>())).ReturnsAsync(history);
            EventUnitOfWork.Setup(_ => _.Save()).ReturnsAsync(true);

            var result = await Subject.StartTaskItem(99, 5).ConfigureAwait(false);

            Assert.IsTrue(result);
            EventUnitOfWork.Verify(_ => _.Save(), Times.Once);

            EventHistoryRepository.Verify(_ => _.CreateHistory(99, It.Is<EventHistory>
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
            Assert.ThrowsAsync<ArgumentException>(async () => await Subject.StartBreakSession(99, duration).ConfigureAwait(false));
        }

        [Test]
        public async Task StartBreakSessionShouldReturnFalseWhenFailedToRecordPromptResponse()
        {
            EventUnitOfWork.Setup(_ => _.Save()).ReturnsAsync(false);

            var result = await Subject.StartBreakSession(99, 300000).ConfigureAwait(false);

            Assert.IsFalse(result);
            EventUnitOfWork.Verify(_ => _.Save(), Times.Once);

            EventPromptRepository.Verify(_ => _.CreatePrompt(99, It.Is<EventPrompt>
            (
                prompt => prompt.PromptType == PromptType.ScheduledBreak && prompt.ConfirmType == PromptConfirmType.Commenced
            )), Times.Once);
        }

        [Test]
        public async Task StartBreakSessionShouldReturnFalseWhenBreakSessionIsOngoing()
        {
            var history = new EventHistory { EventType = EventType.Break };
            EventHistoryRepository.Setup(_ => _.GetLastHistory(It.IsAny<long>(), It.IsAny<DateTime?>(), It.IsAny<bool>())).ReturnsAsync(history);
            EventUnitOfWork.Setup(_ => _.Save()).ReturnsAsync(true);

            var result = await Subject.StartBreakSession(99, 300000).ConfigureAwait(false);

            Assert.IsFalse(result);
            EventHistoryRepository.Verify(_ => _.GetLastHistory(99, null, It.IsAny<bool>()), Times.Once);
            EventHistoryRepository.Verify(_ => _.CreateHistory(It.IsAny<long>(), It.IsAny<EventHistory>()), Times.Never);
            EventUnitOfWork.Verify(_ => _.Save(), Times.Once);

            EventPromptRepository.Verify(_ => _.CreatePrompt(99, It.Is<EventPrompt>
            (
                prompt => prompt.PromptType == PromptType.ScheduledBreak && prompt.ConfirmType == PromptConfirmType.Commenced
            )), Times.Once);
        }

        [Test]
        public async Task StartBreakSessionShouldReturnFalseWhenFailedToStartSession()
        {
            EventHistoryRepository.Setup(_ => _.GetLastHistory(It.IsAny<long>(), It.IsAny<DateTime?>(), It.IsAny<bool>())).ReturnsAsync((EventHistory)null);
            EventUnitOfWork.SetupSequence(_ => _.Save()).ReturnsAsync(true).ReturnsAsync(false);

            var result = await Subject.StartBreakSession(99, 300000).ConfigureAwait(false);

            Assert.IsFalse(result);
            EventUnitOfWork.Verify(_ => _.Save(), Times.Exactly(2));

            EventHistoryRepository.Verify(_ => _.CreateHistory(99, It.Is<EventHistory>
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
            EventHistoryRepository.Setup(_ => _.GetLastHistory(It.IsAny<long>(), It.IsAny<DateTime?>(), It.IsAny<bool>())).ReturnsAsync(history);
            EventUnitOfWork.SetupSequence(_ => _.Save()).ReturnsAsync(true).ReturnsAsync(true);

            var result = await Subject.StartBreakSession(99, 300000).ConfigureAwait(false);

            Assert.IsTrue(result);
            EventUnitOfWork.Verify(_ => _.Save(), Times.Exactly(2));

            EventHistoryRepository.Verify(_ => _.CreateHistory(99, It.Is<EventHistory>
            (
                history => history.ResourceId == -1 &&
                           history.EventType == EventType.Break &&
                           history.TargetDuration == 300000
            )), Times.Once);
        }

        [Test]
        public async Task SkipBreakSessionShouldReturnFalseWhenFailedToSkipSession()
        {
            EventUnitOfWork.Setup(_ => _.Save()).ReturnsAsync(false);

            var result = await Subject.SkipBreakSession(99).ConfigureAwait(false);

            Assert.IsFalse(result);
            EventUnitOfWork.Verify(_ => _.Save(), Times.Once);

            EventPromptRepository.Verify(_ => _.CreatePrompt(99, It.Is<EventPrompt>
            (
                prompt => prompt.PromptType == PromptType.ScheduledBreak && prompt.ConfirmType == PromptConfirmType.Skipped
            )), Times.Once);
        }

        [Test]
        public async Task SkipBreakSessionShouldReturnTrueWhenSuccessfullySkippedSession()
        {
            EventUnitOfWork.Setup(_ => _.Save()).ReturnsAsync(true);

            var result = await Subject.SkipBreakSession(99).ConfigureAwait(false);

            Assert.IsTrue(result);
            EventUnitOfWork.Verify(_ => _.Save(), Times.Once);

            EventPromptRepository.Verify(_ => _.CreatePrompt(99, It.Is<EventPrompt>
            (
                prompt => prompt.PromptType == PromptType.ScheduledBreak && prompt.ConfirmType == PromptConfirmType.Skipped
            )), Times.Once);
        }
    }
}
