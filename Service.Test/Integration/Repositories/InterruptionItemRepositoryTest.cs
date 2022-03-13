using Core.DbContexts;
using Core.Enums;
using Core.Models.WorkItem;
using NUnit.Framework;
using Service.Repositories;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Service.Test.Integration.Repositories
{
    [TestFixture]
    [Category("Integration")]
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
        public async Task GetUnresolvedItemSummariesShouldReturnEmptyCollectionWhenNoItemExists()
        {
            var result = await Subject.GetUnresolvedItemSummaries().ConfigureAwait(false);

            Assert.IsFalse(result.Any());
        }

        [Test]
        public async Task GetUnresolvedItemSummariesShouldReturnSummariesOfUnresolvedItems()
        {
            for (var i = 0; i < 5; ++i)
            {
                var payload = new InterruptionItemBase { Name = $"name_{i}", Description = $"description_{i}", Priority = Priority.Medium };
                var created = Subject.CreateItem(payload);

                if (i == 0 || i == 3)
                {
                    created.ResolvedTime = DateTime.UtcNow;
                }
            }

            await Context.SaveChangesAsync().ConfigureAwait(false);

            var result = await Subject.GetUnresolvedItemSummaries().ConfigureAwait(false);

            Assert.AreEqual(3, result.Count);
            Assert.AreEqual("name_1", result[0].Name);
            Assert.AreEqual("name_2", result[1].Name);
            Assert.AreEqual("name_4", result[2].Name);
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
            var payload = new InterruptionItemBase { Name = "name", Description = "description", Priority = Priority.Low };
            var created = Subject.CreateItem(payload);
            await Context.SaveChangesAsync().ConfigureAwait(false);
            await Subject.DeleteItemById(created.Id).ConfigureAwait(false);
            await Context.SaveChangesAsync().ConfigureAwait(false);

            var result = await Subject.GetItemById(created.Id).ConfigureAwait(false);

            Assert.IsNull(result);
        }

        [Test]
        public async Task GetItemByIdShouldReturnDeletedItemWhenNotExcludingDeletedItem()
        {
            var payload = new InterruptionItemBase { Name = "name", Description = "description", Priority = Priority.Low };
            var created = Subject.CreateItem(payload);
            await Context.SaveChangesAsync().ConfigureAwait(false);
            await Subject.DeleteItemById(created.Id).ConfigureAwait(false);
            await Context.SaveChangesAsync().ConfigureAwait(false);

            var result = await Subject.GetItemById(created.Id, false).ConfigureAwait(false);

            Assert.IsTrue(result.IsDeleted);
            Assert.AreEqual(1, result.Id);
        }

        [Test]
        public async Task GetItemByIdShouldReturnItemFound()
        {
            var payload = new InterruptionItemBase { Name = "name", Description = "description", Priority = Priority.Low };
            var created = Subject.CreateItem(payload);
            await Context.SaveChangesAsync().ConfigureAwait(false);

            var result = await Subject.GetItemById(created.Id).ConfigureAwait(false);

            Assert.IsFalse(result.IsDeleted);
            Assert.AreEqual(1, result.Id);
            Assert.AreEqual(DateTimeKind.Utc, result.CreationTime.Kind);
            Assert.AreEqual(DateTimeKind.Utc, result.ModifiedTime.Kind);
        }

        [Test]
        public async Task CreateItemShouldReturnItemCreated()
        {
            var payload = new InterruptionItemBase { Name = "item_name", Description = "item_description", Priority = Priority.Medium };

            var result = Subject.CreateItem(payload);
            await Context.SaveChangesAsync().ConfigureAwait(false);

            Assert.AreEqual(1, result.Id);
            Assert.AreEqual("item_name", result.Name);
            Assert.AreEqual("item_description", result.Description);
            Assert.AreEqual(Priority.Medium, result.Priority);
            Assert.AreEqual(result.CreationTime, result.ModifiedTime);
            Assert.IsNull(result.ResolvedTime);
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
            var payload = new InterruptionItemBase { Name = "previous_name", Description = "previous_description", Priority = Priority.High };
            var item = Subject.CreateItem(payload);
            await Context.SaveChangesAsync().ConfigureAwait(false);

            item.Name = "current_name";
            item.Description = "current_description";
            item.Priority = Priority.Medium;
            var result = await Subject.UpdateItem(item).ConfigureAwait(false);
            await Context.SaveChangesAsync().ConfigureAwait(false);

            Assert.AreEqual("current_name", result.Name);
            Assert.AreEqual("current_description", result.Description);
            Assert.AreEqual(Priority.Medium, result.Priority);
            Assert.IsTrue(result.ModifiedTime > result.CreationTime);
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
            var payload = new InterruptionItemBase { Name = "name", Description = "description", Priority = Priority.Low };
            var created = Subject.CreateItem(payload);
            await Context.SaveChangesAsync().ConfigureAwait(false);
            await Subject.DeleteItemById(created.Id).ConfigureAwait(false);
            await Context.SaveChangesAsync().ConfigureAwait(false);

            var result = await Subject.DeleteItemById(created.Id).ConfigureAwait(false);

            Assert.IsFalse(result);
        }

        [Test]
        public async Task DeleteItemByIdShouldReturnTrueWhenSuccessfullyDeletedItem()
        {
            var payload = new InterruptionItemBase { Name = "name", Description = "description", Priority = Priority.Low };
            var created = Subject.CreateItem(payload);
            await Context.SaveChangesAsync().ConfigureAwait(false);

            var result = await Subject.DeleteItemById(created.Id).ConfigureAwait(false);
            await Context.SaveChangesAsync().ConfigureAwait(false);

            Assert.IsTrue(result);
        }

        [TearDown]
        public async Task TearDown()
        {
            await Context.Database.EnsureDeletedAsync().ConfigureAwait(false);
        }
    }
}
