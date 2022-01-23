using Core.Dtos;
using Core.Interfaces.Services;
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
        private Mock<IEventService> EventService { get; set; }
        private HttpClient HttpClient { get; set; }

        [SetUp]
        public async Task Setup()
        {
            EventService = new Mock<IEventService>();
            HttpClient = await new ControllerTestUtility().SetupTestHttpClient(_ => _.AddSingleton(EventService.Object)).ConfigureAwait(false);
        }

        [Test]
        public async Task GetOngoingTimeSummaryShouldReturnSummary()
        {
            var time = DateTime.Now.AddHours(-10);
            EventService.Setup(_ => _.GetOngoingTimeSummary(It.IsAny<DateTime>())).ReturnsAsync(new OngoingEventTimeSummaryDto());

            var response = await HttpClient.GetAsync($"{ApiBase}/time-summary/{time:o}").ConfigureAwait(false);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.IsNotNull(await response.Content.ReadFromJsonAsync<OngoingEventTimeSummaryDto>().ConfigureAwait(false));
            EventService.Verify(_ => _.GetOngoingTimeSummary(time), Times.Once);
        }

        [Test]
        public async Task StartIdlingSessionShouldStartIdlingSession()
        {
            EventService.Setup(_ => _.StartIdlingSession()).ReturnsAsync(true);

            var response = await HttpClient.PostAsync($"{ApiBase}/idling-sessions", null).ConfigureAwait(false);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("true", await response.Content.ReadAsStringAsync().ConfigureAwait(false));
            EventService.Verify(_ => _.StartIdlingSession(), Times.Once);
        }

        [Test]
        public async Task StartInterruptionItemShouldStartInterruptionItem()
        {
            EventService.Setup(_ => _.StartInterruptionItem(It.IsAny<long>())).ReturnsAsync(true);

            var response = await HttpClient.PostAsync($"{ApiBase}/interruption-items/5", null).ConfigureAwait(false);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("true", await response.Content.ReadAsStringAsync().ConfigureAwait(false));
            EventService.Verify(_ => _.StartInterruptionItem(5), Times.Once);
        }

        [Test]
        public async Task StartTaskItemShouldStartTaskItem()
        {
            EventService.Setup(_ => _.StartTaskItem(It.IsAny<long>())).ReturnsAsync(true);

            var response = await HttpClient.PostAsync($"{ApiBase}/task-items/5", null).ConfigureAwait(false);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("true", await response.Content.ReadAsStringAsync().ConfigureAwait(false));
            EventService.Verify(_ => _.StartTaskItem(5), Times.Once);
        }

        [Test]
        public async Task ConfirmBreakSessionPromptShouldReturnBadRequestOnFailure()
        {
            var body = new BreakSessionConfirmationDto { IsSkip = true };
            EventService.Setup(_ => _.SkipBreakSession()).ThrowsAsync(new ArgumentException());

            var response = await HttpClient.PostAsJsonAsync($"{ApiBase}/scheduled-break-prompts", body).ConfigureAwait(false);

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Test]
        public async Task ConfirmBreakSessionPromptShouldStartBreakSessionWhenApplicable()
        {
            var body = new BreakSessionConfirmationDto { IsSkip = false, TargetDuration = 500000 };
            EventService.Setup(_ => _.StartBreakSession(It.IsAny<int>())).ReturnsAsync(true);

            var response = await HttpClient.PostAsJsonAsync($"{ApiBase}/scheduled-break-prompts", body).ConfigureAwait(false);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("true", await response.Content.ReadAsStringAsync().ConfigureAwait(false));
            EventService.Verify(_ => _.StartBreakSession(500000), Times.Once);
        }

        [Test]
        public async Task ConfirmBreakSessionPromptShouldSkipBreakSessionWhenApplicable()
        {
            var body = new BreakSessionConfirmationDto { IsSkip = true };
            EventService.Setup(_ => _.SkipBreakSession()).ReturnsAsync(true);

            var response = await HttpClient.PostAsJsonAsync($"{ApiBase}/scheduled-break-prompts", body).ConfigureAwait(false);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("true", await response.Content.ReadAsStringAsync().ConfigureAwait(false));
            EventService.Verify(_ => _.SkipBreakSession(), Times.Once);
        }
    }
}
