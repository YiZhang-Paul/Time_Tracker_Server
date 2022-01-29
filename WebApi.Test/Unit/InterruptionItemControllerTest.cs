using Core.Dtos;
using Core.Enums;
using Core.Interfaces.Repositories;
using Core.Interfaces.Services;
using Core.Models.Interruption;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace WebApi.Test.Unit
{
    [TestFixture]
    public class InterruptionItemControllerTest
    {
        private const string ApiBase = "api/v1/interruption-items";
        private Mock<IInterruptionItemRepository> InterruptionItemRepository { get; set; }
        private Mock<IInterruptionItemService> InterruptionItemService { get; set; }
        private HttpClient HttpClient { get; set; }

        [SetUp]
        public async Task Setup()
        {
            InterruptionItemRepository = new Mock<IInterruptionItemRepository>();
            InterruptionItemService = new Mock<IInterruptionItemService>();

            HttpClient = await new ControllerTestUtility().SetupTestHttpClient
            (
                _ => _.AddSingleton(InterruptionItemRepository.Object)
                      .AddSingleton(InterruptionItemService.Object)
            ).ConfigureAwait(false);
        }

        [Test]
        public async Task GetUnresolvedItemSummariesShouldReturnSummaries()
        {
            var summaries = new ItemSummariesDto
            {
                Resolved = new List<InterruptionItemSummaryDto>
                {
                    new InterruptionItemSummaryDto(),
                    new InterruptionItemSummaryDto()
                },
                Unresolved = new List<InterruptionItemSummaryDto>
                {
                    new InterruptionItemSummaryDto(),
                    new InterruptionItemSummaryDto(),
                    new InterruptionItemSummaryDto()
                }
            };

            var time = DateTime.Now.AddHours(-10);
            InterruptionItemService.Setup(_ => _.GetItemSummaries(It.IsAny<DateTime>())).ReturnsAsync(summaries);

            var response = await HttpClient.GetAsync($"{ApiBase}/summaries/{time:o}").ConfigureAwait(false);
            var result = await response.Content.ReadFromJsonAsync<ItemSummariesDto>().ConfigureAwait(false);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual(2, result.Resolved.Count);
            Assert.AreEqual(3, result.Unresolved.Count);
            InterruptionItemService.Verify(_ => _.GetItemSummaries(time), Times.Once);
        }

        [Test]
        public async Task GetItemByIdShouldReturnItem()
        {
            InterruptionItemRepository.Setup(_ => _.GetItemById(It.IsAny<long>(), It.IsAny<bool>())).ReturnsAsync(new InterruptionItem());

            var response = await HttpClient.GetAsync($"{ApiBase}/5").ConfigureAwait(false);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.IsNotNull(await response.Content.ReadFromJsonAsync<InterruptionItem>().ConfigureAwait(false));
            InterruptionItemRepository.Verify(_ => _.GetItemById(5, true), Times.Once);
        }

        [Test]
        public async Task CreateItemShouldReturnBadRequestWhenItemNameIsNull()
        {
            var response = await HttpClient.PostAsJsonAsync(ApiBase, new InterruptionItemCreationDto { Name = null }).ConfigureAwait(false);

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            InterruptionItemRepository.Verify(_ => _.CreateItem(It.IsAny<InterruptionItemCreationDto>()), Times.Never);
        }

        [Test]
        public async Task CreateItemShouldReturnBadRequestWhenItemNameIsEmpty()
        {
            var response = await HttpClient.PostAsJsonAsync(ApiBase, new InterruptionItemCreationDto { Name = " " }).ConfigureAwait(false);

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            InterruptionItemRepository.Verify(_ => _.CreateItem(It.IsAny<InterruptionItemCreationDto>()), Times.Never);
        }

        [Test]
        public async Task CreateItemShouldReturnItemCreated()
        {
            InterruptionItemRepository.Setup(_ => _.CreateItem(It.IsAny<InterruptionItemCreationDto>())).ReturnsAsync(new InterruptionItem());

            var response = await HttpClient.PostAsJsonAsync(ApiBase, new InterruptionItemCreationDto { Name = "item_name" }).ConfigureAwait(false);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.IsNotNull(await response.Content.ReadFromJsonAsync<InterruptionItem>().ConfigureAwait(false));
            InterruptionItemRepository.Verify(_ => _.CreateItem(It.IsAny<InterruptionItemCreationDto>()), Times.Once);
        }

        [Test]
        public async Task UpdateItemShouldReturnItemUpdated()
        {
            InterruptionItemService.Setup(_ => _.UpdateItem(It.IsAny<InterruptionItem>(), It.IsAny<ResolveAction>())).ReturnsAsync(new InterruptionItem());

            var response = await HttpClient.PutAsJsonAsync($"{ApiBase}?resolve={ResolveAction.Resolve}", new InterruptionItem { Id = 1, Name = "item_name" }).ConfigureAwait(false);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.IsNotNull(await response.Content.ReadFromJsonAsync<InterruptionItem>().ConfigureAwait(false));
            InterruptionItemService.Verify(_ => _.UpdateItem(It.IsAny<InterruptionItem>(), ResolveAction.Resolve), Times.Once);
        }

        [Test]
        public async Task DeleteItemByIdShouldDeleteItem()
        {
            InterruptionItemRepository.Setup(_ => _.DeleteItemById(It.IsAny<long>())).ReturnsAsync(true);

            var response = await HttpClient.DeleteAsync($"{ApiBase}/5").ConfigureAwait(false);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("true", await response.Content.ReadAsStringAsync().ConfigureAwait(false));
            InterruptionItemRepository.Verify(_ => _.DeleteItemById(5), Times.Once);
        }
    }
}
