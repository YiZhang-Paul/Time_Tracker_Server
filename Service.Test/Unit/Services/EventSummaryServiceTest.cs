using Core.Enums;
using Core.Interfaces.Repositories;
using Core.Interfaces.UnitOfWorks;
using Core.Models.Event;
using Moq;
using NUnit.Framework;
using Service.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Service.Test.Unit.Services
{
    [TestFixture]
    public class EventSummaryServiceTest
    {
        private Mock<IEventHistoryRepository> EventHistoryRepository { get; set; }
        private Mock<IEventPromptRepository> EventPromptRepository { get; set; }
        private Mock<IEventUnitOfWork> EventUnitOfWork { get; set; }
        private EventSummaryService Subject { get; set; }

        [SetUp]
        public void Setup()
        {
            EventHistoryRepository = new Mock<IEventHistoryRepository>();
            EventPromptRepository = new Mock<IEventPromptRepository>();
            EventUnitOfWork = new Mock<IEventUnitOfWork>();
            EventUnitOfWork.SetupGet(_ => _.EventHistory).Returns(EventHistoryRepository.Object);
            EventUnitOfWork.SetupGet(_ => _.EventPrompt).Returns(EventPromptRepository.Object);
            Subject = new EventSummaryService(EventUnitOfWork.Object);
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
            EventHistoryRepository.Setup(_ => _.GetLastHistory(It.IsAny<DateTime?>(), It.IsAny<bool>())).ReturnsAsync((EventHistory)null);

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
            EventHistoryRepository.Setup(_ => _.GetLastHistory(It.IsAny<DateTime?>(), It.IsAny<bool>())).ReturnsAsync(new EventHistory { EventType = EventType.Interruption, Timestamp = start.AddMinutes(-30) });
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
                .ReturnsAsync(new EventHistory { Timestamp = now.AddMinutes(-30) })
                .ReturnsAsync(new EventHistory { Timestamp = now.AddMinutes(-30) });

            var result = await Subject.GetOngoingTimeSummary(now.AddMinutes(-60)).ConfigureAwait(false);

            Assert.AreEqual(now.AddMinutes(-30), result.UnconcludedSinceStart.Timestamp);
            Assert.AreEqual(now.AddMinutes(-10), result.UnconcludedSinceLastBreakPrompt.Timestamp);
        }
    }
}
