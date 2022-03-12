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
        private EventTrackingService Subject { get; set; }

        [SetUp]
        public void Setup()
        {
            EventHistoryRepository = new Mock<IEventHistoryRepository>();
            Subject = new EventTrackingService(EventHistoryRepository.Object);
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
    }
}
