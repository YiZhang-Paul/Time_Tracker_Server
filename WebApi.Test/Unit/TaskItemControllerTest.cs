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
    public class TaskItemControllerTest
    {
        private const string ApiBase = "api/v1/task-items";
        private Mock<ITaskItemRepository> TaskItemRepository { get; set; }
        private Mock<IWorkItemUnitOfWork> WorkItemUnitOfWork { get; set; }
        private Mock<IUserService> UserService { get; set; }
        private Mock<ITaskItemService> TaskItemService { get; set; }
        private HttpClient HttpClient { get; set; }

        [SetUp]
        public async Task Setup()
        {
            TaskItemRepository = new Mock<ITaskItemRepository>();
            WorkItemUnitOfWork = new Mock<IWorkItemUnitOfWork>();
            UserService = new Mock<IUserService>();
            TaskItemService = new Mock<ITaskItemService>();

            HttpClient = await new ControllerTestUtility().SetupTestHttpClient
            (
                _ => _.AddSingleton(WorkItemUnitOfWork.Object)
                      .AddSingleton(UserService.Object)
                      .AddSingleton(TaskItemService.Object)
            ).ConfigureAwait(false);

            WorkItemUnitOfWork.SetupGet(_ => _.TaskItem).Returns(TaskItemRepository.Object);
            UserService.Setup(_ => _.GetProfile(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(new UserProfile { Id = 99 });
        }

        [Test]
        public async Task GetItemSummariesShouldReturnSummaries()
        {
            var summaries = new ItemSummariesDto<TaskItemSummaryDto>
            {
                Resolved = new List<TaskItemSummaryDto>
                {
                    new TaskItemSummaryDto(),
                    new TaskItemSummaryDto()
                },
                Unresolved = new List<TaskItemSummaryDto>
                {
                    new TaskItemSummaryDto(),
                    new TaskItemSummaryDto(),
                    new TaskItemSummaryDto()
                }
            };

            var time = DateTime.UtcNow.AddHours(-10);
            TaskItemService.Setup(_ => _.GetItemSummaries(It.IsAny<long>(), It.IsAny<DateTime>())).ReturnsAsync(summaries);

            var response = await HttpClient.GetAsync($"{ApiBase}/summaries/{time:o}").ConfigureAwait(false);
            var result = await response.Content.ReadFromJsonAsync<ItemSummariesDto<TaskItemSummaryDto>>().ConfigureAwait(false);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual(2, result.Resolved.Count);
            Assert.AreEqual(3, result.Unresolved.Count);
            TaskItemService.Verify(_ => _.GetItemSummaries(99, time), Times.Once);
        }

        [Test]
        public async Task GetItemByIdShouldReturnItem()
        {
            TaskItemRepository.Setup(_ => _.GetItemById(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<bool>())).ReturnsAsync(new TaskItem());

            var response = await HttpClient.GetAsync($"{ApiBase}/5").ConfigureAwait(false);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.IsNotNull(await response.Content.ReadFromJsonAsync<TaskItem>().ConfigureAwait(false));
            TaskItemRepository.Verify(_ => _.GetItemById(99, 5, true), Times.Once);
        }

        [Test]
        public async Task CreateItemShouldReturnItemCreated()
        {
            TaskItemService.Setup(_ => _.CreateItem(It.IsAny<long>(), It.IsAny<TaskItemBase>())).ReturnsAsync(new TaskItem());

            var response = await HttpClient.PostAsJsonAsync(ApiBase, new TaskItemBase { Name = "item_name" }).ConfigureAwait(false);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.IsNotNull(await response.Content.ReadFromJsonAsync<TaskItem>().ConfigureAwait(false));
            TaskItemService.Verify(_ => _.CreateItem(99, It.IsAny<TaskItemBase>()), Times.Once);
        }

        [Test]
        public async Task UpdateItemShouldReturnItemUpdated()
        {
            TaskItemService.Setup(_ => _.UpdateItem(It.IsAny<long>(), It.IsAny<TaskItem>(), It.IsAny<ResolveAction>())).ReturnsAsync(new TaskItem());

            var response = await HttpClient.PutAsJsonAsync($"{ApiBase}?resolve={ResolveAction.Unresolve}", new TaskItem { Id = 1, Name = "item_name" }).ConfigureAwait(false);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.IsNotNull(await response.Content.ReadFromJsonAsync<TaskItem>().ConfigureAwait(false));
            TaskItemService.Verify(_ => _.UpdateItem(99, It.IsAny<TaskItem>(), ResolveAction.Unresolve), Times.Once);
        }

        [Test]
        public async Task DeleteItemByIdShouldDeleteItem()
        {
            TaskItemRepository.Setup(_ => _.DeleteItemById(It.IsAny<long>(), It.IsAny<long>())).ReturnsAsync(true);
            WorkItemUnitOfWork.Setup(_ => _.Save()).ReturnsAsync(true);

            var response = await HttpClient.DeleteAsync($"{ApiBase}/5").ConfigureAwait(false);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("true", await response.Content.ReadAsStringAsync().ConfigureAwait(false));
            TaskItemRepository.Verify(_ => _.DeleteItemById(99, 5), Times.Once);
            WorkItemUnitOfWork.Verify(_ => _.Save(), Times.Once);
        }
    }
}
