using Core.DbContexts;
using Core.Models.User;
using Core.Models.WorkItem;
using NUnit.Framework;
using Service.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Service.Test.Integration.Repositories
{
    [TestFixture]
    [Category("Integration")]
    public class TaskItemRepositoryTest
    {
        private List<UserProfile> Users { get; set; }
        private TimeTrackerDbContext Context { get; set; }
        private TaskItemRepository Subject { get; set; }

        [SetUp]
        public async Task Setup()
        {
            Context = await new DatabaseTestUtility().SetupTimeTrackerDbContext().ConfigureAwait(false);
            Subject = new TaskItemRepository(Context);
            await CreateUsers().ConfigureAwait(false);
        }

        [Test]
        public async Task GetUnresolvedItemSummariesShouldReturnEmptyCollectionWhenNoItemExists()
        {
            var result = await Subject.GetUnresolvedItemSummaries(Users[0].Id).ConfigureAwait(false);

            Assert.IsFalse(result.Any());
        }

        [Test]
        public async Task GetUnresolvedItemSummariesShouldReturnSummariesOfExistingItems()
        {
            var created = new List<TaskItem>
            {
                Subject.CreateItem(Users[0].Id, new TaskItemBase { Name = "name_1", Description = "description_1", Effort = 5 }),
                Subject.CreateItem(Users[0].Id, new TaskItemBase { Name = "name_2", Description = "description_2", Effort = 5 }),
                Subject.CreateItem(Users[1].Id, new TaskItemBase { Name = "name_3", Description = "description_3", Effort = 5 }),
                Subject.CreateItem(Users[0].Id, new TaskItemBase { Name = "name_4", Description = "description_4", Effort = 5 }),
                Subject.CreateItem(Users[0].Id, new TaskItemBase { Name = "name_5", Description = "description_5", Effort = 5 })
            };

            created[0].ResolvedTime = DateTime.UtcNow;
            created[3].ResolvedTime = DateTime.UtcNow;
            await Context.SaveChangesAsync().ConfigureAwait(false);

            var result = await Subject.GetUnresolvedItemSummaries(Users[0].Id).ConfigureAwait(false);

            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("name_2", result[0].Name);
            Assert.AreEqual("name_5", result[1].Name);
        }

        [Test]
        public async Task GetItemByIdShouldReturnNullWhenItemDoesNotExist()
        {
            var result = await Subject.GetItemById(Users[0].Id, 1000).ConfigureAwait(false);

            Assert.IsNull(result);
        }

        [Test]
        public async Task GetItemByIdShouldReturnNullWhenItemIsDeleted()
        {
            var payload = new TaskItemBase { Name = "name", Description = "description", Effort = 5 };
            var created = Subject.CreateItem(Users[0].Id, payload);
            await Context.SaveChangesAsync().ConfigureAwait(false);
            await Subject.DeleteItemById(Users[0].Id, created.Id).ConfigureAwait(false);
            await Context.SaveChangesAsync().ConfigureAwait(false);

            var result = await Subject.GetItemById(Users[0].Id, created.Id).ConfigureAwait(false);

            Assert.IsNull(result);
        }

        [Test]
        public async Task GetItemByIdShouldReturnDeletedItemWhenNotExcludingDeletedItem()
        {
            var payload = new TaskItemBase { Name = "name", Description = "description", Effort = 5 };
            var created = Subject.CreateItem(Users[0].Id, payload);
            await Context.SaveChangesAsync().ConfigureAwait(false);
            await Subject.DeleteItemById(Users[0].Id, created.Id).ConfigureAwait(false);
            await Context.SaveChangesAsync().ConfigureAwait(false);

            var result = await Subject.GetItemById(Users[0].Id, created.Id, false).ConfigureAwait(false);

            Assert.IsTrue(result.IsDeleted);
            Assert.AreEqual(Users[0].Id, result.UserId);
            Assert.AreEqual(1, result.Id);
        }

        [Test]
        public async Task GetItemByIdShouldReturnItemFound()
        {
            var payload = new TaskItemBase { Name = "name", Description = "description", Effort = 5 };
            var created = Subject.CreateItem(Users[0].Id, payload);
            await Context.SaveChangesAsync().ConfigureAwait(false);

            var result = await Subject.GetItemById(Users[0].Id, created.Id).ConfigureAwait(false);

            Assert.IsFalse(result.IsDeleted);
            Assert.AreEqual(Users[0].Id, result.UserId);
            Assert.AreEqual(1, result.Id);
            Assert.AreEqual(DateTimeKind.Utc, result.CreationTime.Kind);
            Assert.AreEqual(DateTimeKind.Utc, result.ModifiedTime.Kind);
        }

        [Test]
        public async Task CreateItemShouldReturnItemCreated()
        {
            var payload = new TaskItemBase { Name = "item_name", Description = "item_description", Effort = 5 };

            var result = Subject.CreateItem(Users[0].Id, payload);
            await Context.SaveChangesAsync().ConfigureAwait(false);

            Assert.AreEqual(Users[0].Id, result.UserId);
            Assert.AreEqual(1, result.Id);
            Assert.AreEqual("item_name", result.Name);
            Assert.AreEqual("item_description", result.Description);
            Assert.AreEqual(5, result.Effort);
            Assert.AreEqual(result.CreationTime, result.ModifiedTime);
            Assert.IsNull(result.ResolvedTime);
            Assert.IsTrue((DateTime.UtcNow - result.CreationTime).Duration().TotalMilliseconds < 1000);
        }

        [Test]
        public async Task UpdateItemShouldReturnNullWhenItemDoesNotExist()
        {
            var result = await Subject.UpdateItem(new TaskItem { UserId = Users[0].Id, Id = 1000 }).ConfigureAwait(false);

            Assert.IsNull(result);
        }

        [Test]
        public async Task UpdateItemShouldReturnItemUpdated()
        {
            var payload = new TaskItemBase { Name = "previous_name", Description = "previous_description", Effort = 5 };
            var item = Subject.CreateItem(Users[0].Id, payload);
            await Context.SaveChangesAsync().ConfigureAwait(false);

            item.Name = "current_name";
            item.Description = "current_description";
            item.Effort = 13;
            var result = await Subject.UpdateItem(item).ConfigureAwait(false);
            await Context.SaveChangesAsync().ConfigureAwait(false);

            Assert.AreEqual("current_name", result.Name);
            Assert.AreEqual("current_description", result.Description);
            Assert.AreEqual(13, result.Effort);
            Assert.IsTrue(result.ModifiedTime > result.CreationTime);
            Assert.IsTrue((DateTime.UtcNow - result.ModifiedTime).Duration().TotalMilliseconds < 1000);
        }

        [Test]
        public async Task DeleteItemByIdShouldReturnFalseWhenItemDoesNotExist()
        {
            var result = await Subject.DeleteItemById(Users[0].Id, 1000).ConfigureAwait(false);

            Assert.IsFalse(result);
        }

        [Test]
        public async Task DeleteItemByIdShouldReturnFalseWhenItemIsAlreadyDeleted()
        {
            var payload = new TaskItemBase { Name = "name", Description = "description", Effort = 5 };
            var created = Subject.CreateItem(Users[0].Id, payload);
            await Context.SaveChangesAsync().ConfigureAwait(false);
            await Subject.DeleteItemById(Users[0].Id, created.Id).ConfigureAwait(false);
            await Context.SaveChangesAsync().ConfigureAwait(false);

            var result = await Subject.DeleteItemById(Users[0].Id, created.Id).ConfigureAwait(false);

            Assert.IsFalse(result);
        }

        [Test]
        public async Task DeleteItemByIdShouldReturnTrueWhenSuccessfullyDeletedItem()
        {
            var payload = new TaskItemBase { Name = "name", Description = "description", Effort = 5 };
            var created = Subject.CreateItem(Users[0].Id, payload);
            await Context.SaveChangesAsync().ConfigureAwait(false);

            var result = await Subject.DeleteItemById(Users[0].Id, created.Id).ConfigureAwait(false);
            await Context.SaveChangesAsync().ConfigureAwait(false);

            Assert.IsTrue(result);
        }

        [TearDown]
        public async Task TearDown()
        {
            await Context.Database.EnsureDeletedAsync().ConfigureAwait(false);
        }

        private async Task CreateUsers()
        {
            var repository = new UserProfileRepository(Context);

            Users = new List<UserProfile>
            {
                repository.CreateProfile(new UserProfile { Email = "john.doe@ymail.com", DisplayName = "John Doe" }),
                repository.CreateProfile(new UserProfile { Email = "jane.doe@ymail.com", DisplayName = "Jane Doe" })
            };

            await Context.SaveChangesAsync().ConfigureAwait(false);
        }
    }
}
