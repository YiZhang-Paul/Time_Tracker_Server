using Core.Interfaces.Repositories;
using Core.Models.WorkItem;
using Moq;
using NUnit.Framework;
using Service.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Service.Test.Unit.Services
{
    [TestFixture]
    public class TaskItemServiceTest
    {
        private Mock<ITaskItemRepository> TaskItemRepository { get; set; }
        private TaskItemService Subject { get; set; }

        [SetUp]
        public void Setup()
        {
            TaskItemRepository = new Mock<ITaskItemRepository>();
            Subject = new TaskItemService(TaskItemRepository.Object);
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public void CreateItemShouldThrowWhenItemNameIsInvalid(string name)
        {
            Assert.ThrowsAsync<ArgumentException>(async () => await Subject.CreateItem(new TaskItemBase { Name = name }).ConfigureAwait(false));
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public void CreateItemShouldThrowWhenChecklistEntryIsInvalid(string description)
        {
            var item = new TaskItemBase
            {
                Name = "valid_name",
                Checklists = new List<TaskChecklistEntry>
                {
                    new TaskChecklistEntry { Description = "valid_description" },
                    new TaskChecklistEntry { Description = description }
                }
            };

            Assert.ThrowsAsync<ArgumentException>(async () => await Subject.CreateItem(item).ConfigureAwait(false));
        }

        [Test]
        public async Task CreateItemShouldReturnItemCreated()
        {
            TaskItemRepository.Setup(_ => _.CreateItem(It.IsAny<TaskItemBase>())).ReturnsAsync(new TaskItem());

            var result = await Subject.CreateItem(new TaskItemBase { Name = "valid_name" }).ConfigureAwait(false);

            Assert.IsNotNull(result);
            TaskItemRepository.Verify(_ => _.CreateItem(It.IsAny<TaskItemBase>()), Times.Once);
        }
    }
}
