using Core.Dtos;
using Core.Interfaces.Repositories;
using Core.Interfaces.Services;
using Core.Models.WorkItem;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace WebApi.Test.Unit
{
    [TestFixture]
    public class EventControllerTest
    {
        private const string ApiBase = "api/v1/events";
        private Mock<IInterruptionItemRepository> InterruptionItemRepository { get; set; }
        private Mock<ITaskItemRepository> TaskItemRepository { get; set; }
        private Mock<IInterruptionItemService> InterruptionItemService { get; set; }
        private Mock<ITaskItemService> TaskItemService { get; set; }
        private Mock<IEventSummaryService> EventSummaryService { get; set; }
        private HttpClient HttpClient { get; set; }

        [SetUp]
        public async Task Setup()
        {
            InterruptionItemRepository = new Mock<IInterruptionItemRepository>();
            TaskItemRepository = new Mock<ITaskItemRepository>();
            InterruptionItemService = new Mock<IInterruptionItemService>();
            TaskItemService = new Mock<ITaskItemService>();
            EventSummaryService = new Mock<IEventSummaryService>();

            HttpClient = await new ControllerTestUtility().SetupTestHttpClient
            (
                _ => _.AddSingleton(InterruptionItemRepository.Object)
                      .AddSingleton(TaskItemRepository.Object)
                      .AddSingleton(InterruptionItemService.Object)
                      .AddSingleton(TaskItemService.Object)
                      .AddSingleton(EventSummaryService.Object)
            ).ConfigureAwait(false);
        }

        [Test]
        public async Task GetOngoingTimeSummaryShouldReturnSummary()
        {
            var time = DateTime.UtcNow.AddHours(-10);
            EventSummaryService.Setup(_ => _.GetOngoingTimeSummary(It.IsAny<DateTime>())).ReturnsAsync(new OngoingEventTimeSummaryDto());

            var response = await HttpClient.GetAsync($"{ApiBase}/time-summary/{time:o}").ConfigureAwait(false);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.IsNotNull(await response.Content.ReadFromJsonAsync<OngoingEventTimeSummaryDto>().ConfigureAwait(false));
            EventSummaryService.Verify(_ => _.GetOngoingTimeSummary(time), Times.Once);
        }

        [Test]
        public async Task StartIdlingSessionShouldStartIdlingSession()
        {
            EventSummaryService.Setup(_ => _.StartIdlingSession()).ReturnsAsync(true);

            var response = await HttpClient.PostAsync($"{ApiBase}/idling-sessions", null).ConfigureAwait(false);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("true", await response.Content.ReadAsStringAsync().ConfigureAwait(false));
            EventSummaryService.Verify(_ => _.StartIdlingSession(), Times.Once);
        }

        [Test]
        public async Task StartInterruptionItemShouldReturnFalseWhenItemDoesNotExist()
        {
            InterruptionItemRepository.Setup(_ => _.GetItemById(It.IsAny<long>(), true)).ReturnsAsync((InterruptionItem)null);

            var response = await HttpClient.PostAsync($"{ApiBase}/interruption-items/5", null).ConfigureAwait(false);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("false", await response.Content.ReadAsStringAsync().ConfigureAwait(false));
            InterruptionItemRepository.Verify(_ => _.GetItemById(5, true), Times.Once);
            EventSummaryService.Verify(_ => _.StartInterruptionItem(It.IsAny<long>()), Times.Never);
        }

        [Test]
        public async Task StartInterruptionItemShouldStartInterruptionItem()
        {
            InterruptionItemRepository.Setup(_ => _.GetItemById(It.IsAny<long>(), true)).ReturnsAsync(new InterruptionItem());
            EventSummaryService.Setup(_ => _.StartInterruptionItem(It.IsAny<long>())).ReturnsAsync(true);

            var response = await HttpClient.PostAsync($"{ApiBase}/interruption-items/5", null).ConfigureAwait(false);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("true", await response.Content.ReadAsStringAsync().ConfigureAwait(false));
            InterruptionItemRepository.Verify(_ => _.GetItemById(5, true), Times.Once);
            EventSummaryService.Verify(_ => _.StartInterruptionItem(5), Times.Once);
        }

        [Test]
        public async Task StartTaskItemShouldReturnFalseWhenItemDoesNotExist()
        {
            TaskItemRepository.Setup(_ => _.GetItemById(It.IsAny<long>(), true)).ReturnsAsync((TaskItem)null);

            var response = await HttpClient.PostAsync($"{ApiBase}/task-items/5", null).ConfigureAwait(false);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("false", await response.Content.ReadAsStringAsync().ConfigureAwait(false));
            TaskItemRepository.Verify(_ => _.GetItemById(5, true), Times.Once);
            EventSummaryService.Verify(_ => _.StartTaskItem(It.IsAny<long>()), Times.Never);
        }

        [Test]
        public async Task StartTaskItemShouldStartTaskItem()
        {
            TaskItemRepository.Setup(_ => _.GetItemById(It.IsAny<long>(), true)).ReturnsAsync(new TaskItem());
            EventSummaryService.Setup(_ => _.StartTaskItem(It.IsAny<long>())).ReturnsAsync(true);

            var response = await HttpClient.PostAsync($"{ApiBase}/task-items/5", null).ConfigureAwait(false);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("true", await response.Content.ReadAsStringAsync().ConfigureAwait(false));
            TaskItemRepository.Verify(_ => _.GetItemById(5, true), Times.Once);
            EventSummaryService.Verify(_ => _.StartTaskItem(5), Times.Once);
        }

        [Test]
        public async Task ConfirmBreakSessionPromptShouldReturnBadRequestOnFailure()
        {
            var body = new BreakSessionConfirmationDto { IsSkip = true };
            EventSummaryService.Setup(_ => _.SkipBreakSession()).ThrowsAsync(new ArgumentException());

            var response = await HttpClient.PostAsJsonAsync($"{ApiBase}/scheduled-break-prompts", body).ConfigureAwait(false);

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Test]
        public async Task ConfirmBreakSessionPromptShouldStartBreakSessionWhenApplicable()
        {
            var body = new BreakSessionConfirmationDto { IsSkip = false, TargetDuration = 500000 };
            EventSummaryService.Setup(_ => _.StartBreakSession(It.IsAny<int>())).ReturnsAsync(true);

            var response = await HttpClient.PostAsJsonAsync($"{ApiBase}/scheduled-break-prompts", body).ConfigureAwait(false);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("true", await response.Content.ReadAsStringAsync().ConfigureAwait(false));
            EventSummaryService.Verify(_ => _.StartBreakSession(500000), Times.Once);
        }

        [Test]
        public async Task ConfirmBreakSessionPromptShouldSkipBreakSessionWhenApplicable()
        {
            var body = new BreakSessionConfirmationDto { IsSkip = true };
            EventSummaryService.Setup(_ => _.SkipBreakSession()).ReturnsAsync(true);

            var response = await HttpClient.PostAsJsonAsync($"{ApiBase}/scheduled-break-prompts", body).ConfigureAwait(false);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("true", await response.Content.ReadAsStringAsync().ConfigureAwait(false));
            EventSummaryService.Verify(_ => _.SkipBreakSession(), Times.Once);
        }
    }
}
