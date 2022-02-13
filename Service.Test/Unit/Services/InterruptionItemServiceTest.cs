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
    public class InterruptionItemServiceTest
    {
        private Mock<IInterruptionItemRepository> InterruptionItemRepository { get; set; }
        private InterruptionItemService Subject { get; set; }

        [SetUp]
        public void Setup()
        {
            InterruptionItemRepository = new Mock<IInterruptionItemRepository>();
            Subject = new InterruptionItemService(InterruptionItemRepository.Object);
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public void CreateItemShouldThrowWhenItemNameIsInvalid(string name)
        {
            Assert.ThrowsAsync<ArgumentException>(async() => await Subject.CreateItem(new InterruptionItemBase { Name = name }).ConfigureAwait(false));
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

            Assert.ThrowsAsync<ArgumentException>(async () => await Subject.CreateItem(item).ConfigureAwait(false));
        }

        [Test]
        public async Task CreateItemShouldReturnItemCreated()
        {
            InterruptionItemRepository.Setup(_ => _.CreateItem(It.IsAny<InterruptionItemBase>())).ReturnsAsync(new InterruptionItem());

            var result = await Subject.CreateItem(new InterruptionItemBase { Name = "valid_name" }).ConfigureAwait(false);

            Assert.IsNotNull(result);
            InterruptionItemRepository.Verify(_ => _.CreateItem(It.IsAny<InterruptionItemBase>()), Times.Once);
        }
    }
}
