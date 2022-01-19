using Core.DbContexts;
using Core.Dtos;
using Core.Enums;
using Core.Models.Interruption;
using NUnit.Framework;
using Service.Repositories;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Service.Test.Integration.Repositories
{
    [TestFixture]
    public class InterruptionItemRepositoryTest
    {
        private TimeTrackerDbContext Context { get; set; }
        private InterruptionItemRepository Subject { get; set; }

        [SetUp]
        public async Task Setup()
        {
            Context = await new DatabaseTestUtility().SetupTimeTrackerDbContext().ConfigureAwait(false);
            Subject = new InterruptionItemRepository(Context);
        }

        [Test]
        public async Task GetInterruptionItemSummariesShouldReturnEmptyCollectionWhenNoItemExists()
        {
            var result = await Subject.GetInterruptionItemSummaries().ConfigureAwait(false);

            Assert.IsFalse(result.Any());
        }

        [Test]
        public async Task GetInterruptionItemSummariesShouldReturnSummariesOfExistingItems()
        {
            for (var i = 0; i < 3; ++i)
            {
                var payload = new InterruptionItemCreationDto
                {
                    Name = $"name_{i}",
                    Description = $"description_{i}",
                    Priority = Priority.Medium
                };

                await Subject.CreateInterruptionItem(payload).ConfigureAwait(false);
            }

            await Subject.DeleteInterruptionItemById(2).ConfigureAwait(false);

            var result = await Subject.GetInterruptionItemSummaries().ConfigureAwait(false);

            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("name_0", result[0].Name);
            Assert.AreEqual("name_2", result[1].Name);
        }

        [Test]
        public async Task GetInterruptionItemByIdShouldReturnNullWhenItemDoesNotExist()
        {
            var result = await Subject.GetInterruptionItemById(1000).ConfigureAwait(false);

            Assert.IsNull(result);
        }

        [Test]
        public async Task GetInterruptionItemByIdShouldReturnNullWhenItemIsDeleted()
        {
            var payload = new InterruptionItemCreationDto { Name = "name", Description = "description", Priority = Priority.Low };
            await Subject.CreateInterruptionItem(payload).ConfigureAwait(false);
            await Subject.DeleteInterruptionItemById(1).ConfigureAwait(false);

            var result = await Subject.GetInterruptionItemById(1).ConfigureAwait(false);

            Assert.IsNull(result);
        }

        [Test]
        public async Task GetInterruptionItemByIdShouldReturnDeletedItemWhenNotExcludingDeletedItem()
        {
            var payload = new InterruptionItemCreationDto { Name = "name", Description = "description", Priority = Priority.Low };
            await Subject.CreateInterruptionItem(payload).ConfigureAwait(false);
            await Subject.DeleteInterruptionItemById(1).ConfigureAwait(false);

            var result = await Subject.GetInterruptionItemById(1, false).ConfigureAwait(false);

            Assert.AreEqual(1, result.Id);
        }

        [Test]
        public async Task GetInterruptionItemByIdShouldReturnItemFound()
        {
            var payload = new InterruptionItemCreationDto { Name = "name", Description = "description", Priority = Priority.Low };
            await Subject.CreateInterruptionItem(payload).ConfigureAwait(false);

            var result = await Subject.GetInterruptionItemById(1).ConfigureAwait(false);

            Assert.AreEqual(1, result.Id);
        }

        [Test]
        public async Task CreateInterruptionItemShouldReturnItemCreated()
        {
            var payload = new InterruptionItemCreationDto
            {
                Name = "item_name",
                Description = "item_description",
                Priority = Priority.Medium
            };

            var result = await Subject.CreateInterruptionItem(payload).ConfigureAwait(false);

            Assert.AreEqual("item_name", result.Name);
            Assert.AreEqual("item_description", result.Description);
            Assert.AreEqual(Priority.Medium, result.Priority);
            Assert.AreEqual(result.CreationTime, result.ModifiedTime);
            Assert.IsTrue((DateTime.UtcNow - result.CreationTime).Duration().TotalMilliseconds < 1000);
        }

        [Test]
        public async Task UpdateInterruptionItemShouldReturnNullWhenItemDoesNotExist()
        {
            var result = await Subject.UpdateInterruptionItem(new InterruptionItem { Id = 1000 }).ConfigureAwait(false);

            Assert.IsNull(result);
        }

        [Test]
        public async Task UpdateInterruptionItemShouldReturnItemUpdated()
        {
            var payload = new InterruptionItemCreationDto
            {
                Name = "previous_name",
                Description = "previous_description",
                Priority = Priority.High
            };

            await Subject.CreateInterruptionItem(payload).ConfigureAwait(false);
            var item = await Subject.GetInterruptionItemById(1).ConfigureAwait(false);
            item.Name = "current_name";
            item.Description = "current_description";
            item.Priority = Priority.Medium;

            var result = await Subject.UpdateInterruptionItem(item).ConfigureAwait(false);

            Assert.AreEqual("current_name", result.Name);
            Assert.AreEqual("current_description", result.Description);
            Assert.AreEqual(Priority.Medium, result.Priority);
            Assert.IsTrue((DateTime.UtcNow - result.ModifiedTime).Duration().TotalMilliseconds < 1000);
        }

        [Test]
        public async Task DeleteInterruptionItemByIdShouldReturnFalseWhenItemDoesNotExist()
        {
            var result = await Subject.DeleteInterruptionItemById(1000).ConfigureAwait(false);

            Assert.IsFalse(result);
        }

        [Test]
        public async Task DeleteInterruptionItemByIdShouldReturnFalseWhenItemIsAlreadyDeleted()
        {
            var payload = new InterruptionItemCreationDto { Name = "name", Description = "description", Priority = Priority.Low };
            await Subject.CreateInterruptionItem(payload).ConfigureAwait(false);
            await Subject.DeleteInterruptionItemById(1).ConfigureAwait(false);

            var result = await Subject.DeleteInterruptionItemById(1).ConfigureAwait(false);

            Assert.IsFalse(result);
        }

        [Test]
        public async Task DeleteInterruptionItemByIdShouldReturnTrueWhenSuccessfullyDeletedItem()
        {
            var payload = new InterruptionItemCreationDto { Name = "name", Description = "description", Priority = Priority.Low };
            await Subject.CreateInterruptionItem(payload).ConfigureAwait(false);

            var result = await Subject.DeleteInterruptionItemById(1).ConfigureAwait(false);

            Assert.IsTrue(result);
        }

        [TearDown]
        public async Task TearDown()
        {
            await Context.Database.EnsureDeletedAsync().ConfigureAwait(false);
        }
    }
}
