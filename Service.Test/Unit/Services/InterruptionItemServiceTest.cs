using Core.Interfaces.Repositories;
using Core.Interfaces.UnitOfWorks;
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
    public class InterruptionItemServiceTest
    {
        private Mock<IInterruptionItemRepository> InterruptionItemRepository { get; set; }
        private Mock<IWorkItemUnitOfWork> WorkItemUnitOfWork { get; set; }
        private InterruptionItemService Subject { get; set; }

        [SetUp]
        public void Setup()
        {
            InterruptionItemRepository = new Mock<IInterruptionItemRepository>();
            WorkItemUnitOfWork = new Mock<IWorkItemUnitOfWork>();
            WorkItemUnitOfWork.SetupGet(_ => _.InterruptionItem).Returns(InterruptionItemRepository.Object);
            Subject = new InterruptionItemService(WorkItemUnitOfWork.Object);
        }

        [Test]
        [TestCase(-1)]
        [TestCase(0)]
        public void CreateItemShouldThrowWhenUserIdIsInvalid(long id)
        {
            var item = new InterruptionItemBase { Name = "valid_name" };

            Assert.ThrowsAsync<ArgumentException>(async () => await Subject.CreateItem(id, item).ConfigureAwait(false));
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public void CreateItemShouldThrowWhenItemNameIsInvalid(string name)
        {
            Assert.ThrowsAsync<ArgumentException>(async() => await Subject.CreateItem(99, new InterruptionItemBase { Name = name }).ConfigureAwait(false));
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public void CreateItemShouldThrowWhenChecklistEntryIsInvalid(string description)
        {
            var item = new InterruptionItemBase
            {
                Name = "valid_name",
                Checklists = new List<InterruptionChecklistEntry>
                {
                    new InterruptionChecklistEntry { Description = "valid_description" },
                    new InterruptionChecklistEntry { Description = description }
                }
            };

            Assert.ThrowsAsync<ArgumentException>(async () => await Subject.CreateItem(99, item).ConfigureAwait(false));
        }

        [Test]
        public async Task CreateItemShouldReturnItemCreated()
        {
            InterruptionItemRepository.Setup(_ => _.CreateItem(It.IsAny<long>(), It.IsAny<InterruptionItemBase>())).Returns(new InterruptionItem());
            WorkItemUnitOfWork.Setup(_ => _.Save()).ReturnsAsync(true);

            var result = await Subject.CreateItem(1, new InterruptionItemBase { Name = "valid_name" }).ConfigureAwait(false);

            Assert.IsNotNull(result);
            InterruptionItemRepository.Verify(_ => _.CreateItem(1, It.IsAny<InterruptionItemBase>()), Times.Once);
            WorkItemUnitOfWork.Verify(_ => _.Save(), Times.Once);
        }
    }
}
