using Core.Dtos;
using Core.Interfaces.Repositories;
using Core.Interfaces.Services;
using Core.Interfaces.UnitOfWorks;
using Core.Models.Authentication;
using Core.Models.WorkItem;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Threading.Tasks;

namespace WebApi.Test.Unit
{
    [TestFixture]
    public class EventControllerTest
    {
        private const string ApiBase = "api/v1/events";
        private Mock<IInterruptionItemRepository> InterruptionItemRepository { get; set; }
        private Mock<ITaskItemRepository> TaskItemRepository { get; set; }
        private Mock<IWorkItemUnitOfWork> WorkItemUnitOfWork { get; set; }
        private Mock<IUserService> UserService { get; set; }
        private Mock<IInterruptionItemService> InterruptionItemService { get; set; }
        private Mock<ITaskItemService> TaskItemService { get; set; }
        private Mock<IEventSummaryService> EventSummaryService { get; set; }
        private Mock<IEventTrackingService> EventTrackingService { get; set; }
        private HttpClient HttpClient { get; set; }

        [SetUp]
        public async Task Setup()
        {
            InterruptionItemRepository = new Mock<IInterruptionItemRepository>();
            TaskItemRepository = new Mock<ITaskItemRepository>();
            WorkItemUnitOfWork = new Mock<IWorkItemUnitOfWork>();
            UserService = new Mock<IUserService>();
            InterruptionItemService = new Mock<IInterruptionItemService>();
            TaskItemService = new Mock<ITaskItemService>();
            EventSummaryService = new Mock<IEventSummaryService>();
            EventTrackingService = new Mock<IEventTrackingService>();

            HttpClient = await new ControllerTestUtility().SetupTestHttpClient
            (
                _ => _.AddSingleton(WorkItemUnitOfWork.Object)
                      .AddSingleton(UserService.Object)
                      .AddSingleton(InterruptionItemService.Object)
                      .AddSingleton(TaskItemService.Object)
                      .AddSingleton(EventSummaryService.Object)
                      .AddSingleton(EventTrackingService.Object)
            ).ConfigureAwait(false);

            WorkItemUnitOfWork.SetupGet(_ => _.InterruptionItem).Returns(InterruptionItemRepository.Object);
            WorkItemUnitOfWork.SetupGet(_ => _.TaskItem).Returns(TaskItemRepository.Object);
            UserService.Setup(_ => _.GetProfile(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(new UserProfile { Id = 99 });
        }

        [Test]
        public async Task GetOngoingTimeSummaryShouldReturnSummary()
        {
            var time = DateTime.UtcNow.AddHours(-10);
            EventSummaryService.Setup(_ => _.GetOngoingTimeSummary(It.IsAny<long>(), It.IsAny<DateTime>())).ReturnsAsync(new OngoingEventTimeSummaryDto());

            var response = await HttpClient.GetAsync($"{ApiBase}/time-summary/{time:o}").ConfigureAwait(false);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.IsNotNull(await response.Content.ReadFromJsonAsync<OngoingEventTimeSummaryDto>().ConfigureAwait(false));
            EventSummaryService.Verify(_ => _.GetOngoingTimeSummary(99, time), Times.Once);
        }

        [Test]
        public async Task StartIdlingSessionShouldStartIdlingSession()
        {
            EventTrackingService.Setup(_ => _.StartIdlingSession(It.IsAny<long>())).ReturnsAsync(true);

            var response = await HttpClient.PostAsync($"{ApiBase}/idling-sessions", null).ConfigureAwait(false);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("true", await response.Content.ReadAsStringAsync().ConfigureAwait(false));
            EventTrackingService.Verify(_ => _.StartIdlingSession(99), Times.Once);
        }

        [Test]
        public async Task StartInterruptionItemShouldReturnFalseWhenItemDoesNotExist()
        {
            InterruptionItemRepository.Setup(_ => _.GetItemById(It.IsAny<long>(), It.IsAny<long>(), true)).ReturnsAsync((InterruptionItem)null);

            var response = await HttpClient.PostAsync($"{ApiBase}/interruption-items/5", null).ConfigureAwait(false);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("false", await response.Content.ReadAsStringAsync().ConfigureAwait(false));
            InterruptionItemRepository.Verify(_ => _.GetItemById(99, 5, true), Times.Once);
            EventTrackingService.Verify(_ => _.StartInterruptionItem(It.IsAny<long>(), It.IsAny<long>()), Times.Never);
        }

        [Test]
        public async Task StartInterruptionItemShouldStartInterruptionItem()
        {
            InterruptionItemRepository.Setup(_ => _.GetItemById(It.IsAny<long>(), It.IsAny<long>(), true)).ReturnsAsync(new InterruptionItem());
            EventTrackingService.Setup(_ => _.StartInterruptionItem(It.IsAny<long>(), It.IsAny<long>())).ReturnsAsync(true);

            var response = await HttpClient.PostAsync($"{ApiBase}/interruption-items/5", null).ConfigureAwait(false);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("true", await response.Content.ReadAsStringAsync().ConfigureAwait(false));
            InterruptionItemRepository.Verify(_ => _.GetItemById(99, 5, true), Times.Once);
            EventTrackingService.Verify(_ => _.StartInterruptionItem(99, 5), Times.Once);
        }

        [Test]
        public async Task StartTaskItemShouldReturnFalseWhenItemDoesNotExist()
        {
            TaskItemRepository.Setup(_ => _.GetItemById(It.IsAny<long>(), It.IsAny<long>(), true)).ReturnsAsync((TaskItem)null);

            var response = await HttpClient.PostAsync($"{ApiBase}/task-items/5", null).ConfigureAwait(false);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("false", await response.Content.ReadAsStringAsync().ConfigureAwait(false));
            TaskItemRepository.Verify(_ => _.GetItemById(99, 5, true), Times.Once);
            EventTrackingService.Verify(_ => _.StartTaskItem(It.IsAny<long>(), It.IsAny<long>()), Times.Never);
        }

        [Test]
        public async Task StartTaskItemShouldStartTaskItem()
        {
            TaskItemRepository.Setup(_ => _.GetItemById(It.IsAny<long>(), It.IsAny<long>(), true)).ReturnsAsync(new TaskItem());
            EventTrackingService.Setup(_ => _.StartTaskItem(It.IsAny<long>(), It.IsAny<long>())).ReturnsAsync(true);

            var response = await HttpClient.PostAsync($"{ApiBase}/task-items/5", null).ConfigureAwait(false);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("true", await response.Content.ReadAsStringAsync().ConfigureAwait(false));
            TaskItemRepository.Verify(_ => _.GetItemById(99, 5, true), Times.Once);
            EventTrackingService.Verify(_ => _.StartTaskItem(99, 5), Times.Once);
        }

        [Test]
        public async Task ConfirmBreakSessionPromptShouldReturnBadRequestOnFailure()
        {
            var body = new BreakSessionConfirmationDto { IsSkip = true };
            EventTrackingService.Setup(_ => _.SkipBreakSession(It.IsAny<long>())).ThrowsAsync(new ArgumentException());

            var response = await HttpClient.PostAsJsonAsync($"{ApiBase}/scheduled-break-prompts", body).ConfigureAwait(false);

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Test]
        public async Task ConfirmBreakSessionPromptShouldStartBreakSessionWhenApplicable()
        {
            var body = new BreakSessionConfirmationDto { IsSkip = false, TargetDuration = 500000 };
            EventTrackingService.Setup(_ => _.StartBreakSession(It.IsAny<long>(), It.IsAny<int>())).ReturnsAsync(true);

            var response = await HttpClient.PostAsJsonAsync($"{ApiBase}/scheduled-break-prompts", body).ConfigureAwait(false);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("true", await response.Content.ReadAsStringAsync().ConfigureAwait(false));
            EventTrackingService.Verify(_ => _.StartBreakSession(99, 500000), Times.Once);
        }

        [Test]
        public async Task ConfirmBreakSessionPromptShouldSkipBreakSessionWhenApplicable()
        {
            var body = new BreakSessionConfirmationDto { IsSkip = true };
            EventTrackingService.Setup(_ => _.SkipBreakSession(It.IsAny<long>())).ReturnsAsync(true);

            var response = await HttpClient.PostAsJsonAsync($"{ApiBase}/scheduled-break-prompts", body).ConfigureAwait(false);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("true", await response.Content.ReadAsStringAsync().ConfigureAwait(false));
            EventTrackingService.Verify(_ => _.SkipBreakSession(99), Times.Once);
        }
    }
}
