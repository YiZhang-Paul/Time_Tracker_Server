using Core.Dtos;
using Core.Interfaces.Repositories;
using Core.Models.Interruption;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
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
        private HttpClient HttpClient { get; set; }

        [SetUp]
        public async Task Setup()
        {
            InterruptionItemRepository = new Mock<IInterruptionItemRepository>();
            HttpClient = await new ControllerTestUtility().SetupTestHttpClient(_ => _.AddSingleton(InterruptionItemRepository.Object)).ConfigureAwait(false);
        }

        [Test]
        public async Task GetItemSummariesShouldReturnSummaries()
        {
            var summaries = new List<InterruptionItemSummaryDto>
            {
                new InterruptionItemSummaryDto(),
                new InterruptionItemSummaryDto(),
                new InterruptionItemSummaryDto()
            };

            InterruptionItemRepository.Setup(_ => _.GetItemSummaries()).ReturnsAsync(summaries);

            var response = await HttpClient.GetAsync($"{ApiBase}/summaries").ConfigureAwait(false);
            var result = await response.Content.ReadFromJsonAsync<List<InterruptionItemSummaryDto>>().ConfigureAwait(false);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual(3, result.Count);
            InterruptionItemRepository.Verify(_ => _.GetItemSummaries(), Times.Once);
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
        public async Task UpdateItemShouldReturnBadRequestWhenItemNameIsNull()
        {
            var response = await HttpClient.PutAsJsonAsync(ApiBase, new InterruptionItem { Id = 1, Name = null }).ConfigureAwait(false);

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            InterruptionItemRepository.Verify(_ => _.UpdateItem(It.IsAny<InterruptionItem>()), Times.Never);
        }

        [Test]
        public async Task UpdateItemShouldReturnBadRequestWhenItemNameIsEmpty()
        {
            var response = await HttpClient.PutAsJsonAsync(ApiBase, new InterruptionItem { Id = 1, Name = " " }).ConfigureAwait(false);

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            InterruptionItemRepository.Verify(_ => _.UpdateItem(It.IsAny<InterruptionItem>()), Times.Never);
        }

        [Test]
        public async Task UpdateItemShouldReturnBadRequestWhenItemIdIsInvalid()
        {
            var response = await HttpClient.PutAsJsonAsync(ApiBase, new InterruptionItem { Id = -1, Name = "item_name" }).ConfigureAwait(false);

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            InterruptionItemRepository.Verify(_ => _.UpdateItem(It.IsAny<InterruptionItem>()), Times.Never);
        }

        [Test]
        public async Task UpdateItemShouldReturnItemUpdated()
        {
            InterruptionItemRepository.Setup(_ => _.UpdateItem(It.IsAny<InterruptionItem>())).ReturnsAsync(new InterruptionItem());

            var response = await HttpClient.PutAsJsonAsync(ApiBase, new InterruptionItem { Id = 1, Name = "item_name" }).ConfigureAwait(false);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.IsNotNull(await response.Content.ReadFromJsonAsync<InterruptionItem>().ConfigureAwait(false));
            InterruptionItemRepository.Verify(_ => _.UpdateItem(It.IsAny<InterruptionItem>()), Times.Once);
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