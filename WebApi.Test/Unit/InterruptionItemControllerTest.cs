using Core.Dtos;
using Core.Enums;
using Core.Interfaces.Repositories;
using Core.Interfaces.Services;
using Core.Interfaces.UnitOfWorks;
using Core.Models.User;
using Core.Models.WorkItem;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Threading.Tasks;

namespace WebApi.Test.Unit
{
    [TestFixture]
    public class InterruptionItemControllerTest
    {
        private const string ApiBase = "api/v1/interruption-items";
        private Mock<IInterruptionItemRepository> InterruptionItemRepository { get; set; }
        private Mock<IWorkItemUnitOfWork> WorkItemUnitOfWork { get; set; }
        private Mock<IUserService> UserService { get; set; }
        private Mock<IInterruptionItemService> InterruptionItemService { get; set; }
        private HttpClient HttpClient { get; set; }

        [SetUp]
        public async Task Setup()
        {
            InterruptionItemRepository = new Mock<IInterruptionItemRepository>();
            WorkItemUnitOfWork = new Mock<IWorkItemUnitOfWork>();
            UserService = new Mock<IUserService>();
            InterruptionItemService = new Mock<IInterruptionItemService>();

            HttpClient = await new ControllerTestUtility().SetupTestHttpClient
            (
                _ => _.AddSingleton(WorkItemUnitOfWork.Object)
                      .AddSingleton(UserService.Object)
                      .AddSingleton(InterruptionItemService.Object)
            ).ConfigureAwait(false);

            WorkItemUnitOfWork.SetupGet(_ => _.InterruptionItem).Returns(InterruptionItemRepository.Object);
            UserService.Setup(_ => _.GetProfile(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(new UserProfile { Id = 99 });
        }

        [Test]
        public async Task GetItemSummariesShouldReturnSummaries()
        {
            var summaries = new ItemSummariesDto<InterruptionItemSummaryDto>
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

            var time = DateTime.UtcNow.AddHours(-10);
            InterruptionItemService.Setup(_ => _.GetItemSummaries(It.IsAny<long>(), It.IsAny<DateTime>())).ReturnsAsync(summaries);

            var response = await HttpClient.GetAsync($"{ApiBase}/summaries/{time:o}").ConfigureAwait(false);
            var result = await response.Content.ReadFromJsonAsync<ItemSummariesDto<InterruptionItemSummaryDto>>().ConfigureAwait(false);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual(2, result.Resolved.Count);
            Assert.AreEqual(3, result.Unresolved.Count);
            InterruptionItemService.Verify(_ => _.GetItemSummaries(99, time), Times.Once);
        }

        [Test]
        public async Task GetItemByIdShouldReturnItem()
        {
            InterruptionItemRepository.Setup(_ => _.GetItemById(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<bool>())).ReturnsAsync(new InterruptionItem());

            var response = await HttpClient.GetAsync($"{ApiBase}/5").ConfigureAwait(false);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.IsNotNull(await response.Content.ReadFromJsonAsync<InterruptionItem>().ConfigureAwait(false));
            InterruptionItemRepository.Verify(_ => _.GetItemById(99, 5, true), Times.Once);
        }

        [Test]
        public async Task CreateItemShouldReturnItemCreated()
        {
            InterruptionItemService.Setup(_ => _.CreateItem(It.IsAny<long>(), It.IsAny<InterruptionItemBase>())).ReturnsAsync(new InterruptionItem());

            var response = await HttpClient.PostAsJsonAsync(ApiBase, new InterruptionItemBase { Name = "item_name" }).ConfigureAwait(false);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.IsNotNull(await response.Content.ReadFromJsonAsync<InterruptionItem>().ConfigureAwait(false));
            InterruptionItemService.Verify(_ => _.CreateItem(99, It.IsAny<InterruptionItemBase>()), Times.Once);
        }

        [Test]
        public async Task UpdateItemShouldReturnItemUpdated()
        {
            InterruptionItemService.Setup(_ => _.UpdateItem(It.IsAny<long>(), It.IsAny<InterruptionItem>(), It.IsAny<ResolveAction>())).ReturnsAsync(new InterruptionItem());

            var response = await HttpClient.PutAsJsonAsync($"{ApiBase}?resolve={ResolveAction.Resolve}", new InterruptionItem { Id = 1, Name = "item_name" }).ConfigureAwait(false);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.IsNotNull(await response.Content.ReadFromJsonAsync<InterruptionItem>().ConfigureAwait(false));
            InterruptionItemService.Verify(_ => _.UpdateItem(99, It.IsAny<InterruptionItem>(), ResolveAction.Resolve), Times.Once);
        }

        [Test]
        public async Task DeleteItemByIdShouldDeleteItem()
        {
            InterruptionItemRepository.Setup(_ => _.DeleteItemById(It.IsAny<long>(), It.IsAny<long>())).ReturnsAsync(true);
            WorkItemUnitOfWork.Setup(_ => _.Save()).ReturnsAsync(true);

            var response = await HttpClient.DeleteAsync($"{ApiBase}/5").ConfigureAwait(false);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("true", await response.Content.ReadAsStringAsync().ConfigureAwait(false));
            InterruptionItemRepository.Verify(_ => _.DeleteItemById(99, 5), Times.Once);
            WorkItemUnitOfWork.Verify(_ => _.Save(), Times.Once);
        }
    }
}
