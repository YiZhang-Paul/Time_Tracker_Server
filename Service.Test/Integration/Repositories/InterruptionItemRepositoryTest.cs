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
        public async Task GetItemSummariesShouldReturnEmptyCollectionWhenNoItemExists()
        {
            var result = await Subject.GetItemSummaries().ConfigureAwait(false);

            Assert.IsFalse(result.Any());
        }

        [Test]
        public async Task GetItemSummariesShouldReturnSummariesOfExistingItems()
        {
            for (var i = 0; i < 3; ++i)
            {
                var payload = new InterruptionItemCreationDto
                {
                    Name = $"name_{i}",
                    Description = $"description_{i}",
                    Priority = Priority.Medium
                };

                await Subject.CreateItem(payload).ConfigureAwait(false);
            }

            await Subject.DeleteItemById(2).ConfigureAwait(false);

            var result = await Subject.GetItemSummaries().ConfigureAwait(false);

            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("name_0", result[0].Name);
            Assert.AreEqual("name_2", result[1].Name);
        }

        [Test]
        public async Task GetItemByIdShouldReturnNullWhenItemDoesNotExist()
        {
            var result = await Subject.GetItemById(1000).ConfigureAwait(false);

            Assert.IsNull(result);
        }

        [Test]
        public async Task GetItemByIdShouldReturnNullWhenItemIsDeleted()
        {
            var payload = new InterruptionItemCreationDto { Name = "name", Description = "description", Priority = Priority.Low };
            await Subject.CreateItem(payload).ConfigureAwait(false);
            await Subject.DeleteItemById(1).ConfigureAwait(false);

            var result = await Subject.GetItemById(1).ConfigureAwait(false);

            Assert.IsNull(result);
        }

        [Test]
        public async Task GetItemByIdShouldReturnDeletedItemWhenNotExcludingDeletedItem()
        {
            var payload = new InterruptionItemCreationDto { Name = "name", Description = "description", Priority = Priority.Low };
            await Subject.CreateItem(payload).ConfigureAwait(false);
            await Subject.DeleteItemById(1).ConfigureAwait(false);

            var result = await Subject.GetItemById(1, false).ConfigureAwait(false);

            Assert.AreEqual(1, result.Id);
        }

        [Test]
        public async Task GetItemByIdShouldReturnItemFound()
        {
            var payload = new InterruptionItemCreationDto { Name = "name", Description = "description", Priority = Priority.Low };
            await Subject.CreateItem(payload).ConfigureAwait(false);

            var result = await Subject.GetItemById(1).ConfigureAwait(false);

            Assert.AreEqual(1, result.Id);
        }

        [Test]
        public async Task CreateItemShouldReturnItemCreated()
        {
            var payload = new InterruptionItemCreationDto
            {
                Name = "item_name",
                Description = "item_description",
                Priority = Priority.Medium
            };

            var result = await Subject.CreateItem(payload).ConfigureAwait(false);

            Assert.AreEqual("item_name", result.Name);
            Assert.AreEqual("item_description", result.Description);
            Assert.AreEqual(Priority.Medium, result.Priority);
            Assert.AreEqual(result.CreationTime, result.ModifiedTime);
            Assert.IsTrue((DateTime.UtcNow - result.CreationTime).Duration().TotalMilliseconds < 1000);
        }

        [Test]
        public async Task UpdateItemShouldReturnNullWhenItemDoesNotExist()
        {
            var result = await Subject.UpdateItem(new InterruptionItem { Id = 1000 }).ConfigureAwait(false);

            Assert.IsNull(result);
        }

        [Test]
        public async Task UpdateItemShouldReturnItemUpdated()
        {
            var payload = new InterruptionItemCreationDto
            {
                Name = "previous_name",
                Description = "previous_description",
                Priority = Priority.High
            };

            await Subject.CreateItem(payload).ConfigureAwait(false);
            var item = await Subject.GetItemById(1).ConfigureAwait(false);
            item.Name = "current_name";
            item.Description = "current_description";
            item.Priority = Priority.Medium;

            var result = await Subject.UpdateItem(item).ConfigureAwait(false);

            Assert.AreEqual("current_name", result.Name);
            Assert.AreEqual("current_description", result.Description);
            Assert.AreEqual(Priority.Medium, result.Priority);
            Assert.IsTrue((DateTime.UtcNow - result.ModifiedTime).Duration().TotalMilliseconds < 1000);
        }

        [Test]
        public async Task DeleteItemByIdShouldReturnFalseWhenItemDoesNotExist()
        {
            var result = await Subject.DeleteItemById(1000).ConfigureAwait(false);

            Assert.IsFalse(result);
        }

        [Test]
        public async Task DeleteItemByIdShouldReturnFalseWhenItemIsAlreadyDeleted()
        {
            var payload = new InterruptionItemCreationDto { Name = "name", Description = "description", Priority = Priority.Low };
            await Subject.CreateItem(payload).ConfigureAwait(false);
            await Subject.DeleteItemById(1).ConfigureAwait(false);

            var result = await Subject.DeleteItemById(1).ConfigureAwait(false);

            Assert.IsFalse(result);
        }

        [Test]
        public async Task DeleteItemByIdShouldReturnTrueWhenSuccessfullyDeletedItem()
        {
            var payload = new InterruptionItemCreationDto { Name = "name", Description = "description", Priority = Priority.Low };
            await Subject.CreateItem(payload).ConfigureAwait(false);

            var result = await Subject.DeleteItemById(1).ConfigureAwait(false);

            Assert.IsTrue(result);
        }

        [TearDown]
        public async Task TearDown()
        {
            await Context.Database.EnsureDeletedAsync().ConfigureAwait(false);
        }
    }
}
