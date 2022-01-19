using Core.DbContexts;
using Core.Dtos;
using Core.Models.Task;
using NUnit.Framework;
using Service.Repositories;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Service.Test.Integration.Repositories
{
    [TestFixture]
    public class TaskItemRepositoryTest
    {
        private TimeTrackerDbContext Context { get; set; }
        private TaskItemRepository Subject { get; set; }

        [SetUp]
        public async Task Setup()
        {
            Context = await new DatabaseTestUtility().SetupTimeTrackerDbContext().ConfigureAwait(false);
            Subject = new TaskItemRepository(Context);
        }

        [Test]
        public async Task GetTaskItemSummariesShouldReturnEmptyCollectionWhenNoItemExists()
        {
            var result = await Subject.GetTaskItemSummaries().ConfigureAwait(false);

            Assert.IsFalse(result.Any());
        }

        [Test]
        public async Task GetTaskItemSummariesShouldReturnSummariesOfExistingItems()
        {
            for (var i = 0; i < 3; ++i)
            {
                var payload = new TaskItemCreationDto
                {
                    Name = $"name_{i}",
                    Description = $"description_{i}",
                    Effort = 5
                };

                await Subject.CreateTaskItem(payload).ConfigureAwait(false);
            }

            await Subject.DeleteTaskItemById(2).ConfigureAwait(false);

            var result = await Subject.GetTaskItemSummaries().ConfigureAwait(false);

            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("name_0", result[0].Name);
            Assert.AreEqual("name_2", result[1].Name);
        }

        [Test]
        public async Task GetTaskItemByIdShouldReturnNullWhenItemDoesNotExist()
        {
            var result = await Subject.GetTaskItemById(1000).ConfigureAwait(false);

            Assert.IsNull(result);
        }

        [Test]
        public async Task GetTaskItemByIdShouldReturnNullWhenItemIsDeleted()
        {
            var payload = new TaskItemCreationDto { Name = "name", Description = "description", Effort = 5 };
            await Subject.CreateTaskItem(payload).ConfigureAwait(false);
            await Subject.DeleteTaskItemById(1).ConfigureAwait(false);

            var result = await Subject.GetTaskItemById(1).ConfigureAwait(false);

            Assert.IsNull(result);
        }

        [Test]
        public async Task GetTaskItemByIdShouldReturnDeletedItemWhenNotExcludingDeletedItem()
        {
            var payload = new TaskItemCreationDto { Name = "name", Description = "description", Effort = 5 };
            await Subject.CreateTaskItem(payload).ConfigureAwait(false);
            await Subject.DeleteTaskItemById(1).ConfigureAwait(false);

            var result = await Subject.GetTaskItemById(1, false).ConfigureAwait(false);

            Assert.AreEqual(1, result.Id);
        }

        [Test]
        public async Task GetTaskItemByIdShouldReturnItemFound()
        {
            var payload = new TaskItemCreationDto { Name = "name", Description = "description", Effort = 5 };
            await Subject.CreateTaskItem(payload).ConfigureAwait(false);

            var result = await Subject.GetTaskItemById(1).ConfigureAwait(false);

            Assert.AreEqual(1, result.Id);
        }

        [Test]
        public async Task CreateTaskItemShouldReturnItemCreated()
        {
            var payload = new TaskItemCreationDto
            {
                Name = "item_name",
                Description = "item_description",
                Effort = 5
            };

            var result = await Subject.CreateTaskItem(payload).ConfigureAwait(false);

            Assert.AreEqual("item_name", result.Name);
            Assert.AreEqual("item_description", result.Description);
            Assert.AreEqual(5, result.Effort);
            Assert.AreEqual(result.CreationTime, result.ModifiedTime);
            Assert.IsTrue((DateTime.UtcNow - result.CreationTime).Duration().TotalMilliseconds < 1000);
        }

        [Test]
        public async Task UpdateTaskItemShouldReturnNullWhenItemDoesNotExist()
        {
            var result = await Subject.UpdateTaskItem(new TaskItem { Id = 1000 }).ConfigureAwait(false);

            Assert.IsNull(result);
        }

        [Test]
        public async Task UpdateTaskItemShouldReturnItemUpdated()
        {
            var payload = new TaskItemCreationDto
            {
                Name = "previous_name",
                Description = "previous_description",
                Effort = 5
            };

            await Subject.CreateTaskItem(payload).ConfigureAwait(false);
            var item = await Subject.GetTaskItemById(1).ConfigureAwait(false);
            item.Name = "current_name";
            item.Description = "current_description";
            item.Effort = 13;

            var result = await Subject.UpdateTaskItem(item).ConfigureAwait(false);

            Assert.AreEqual("current_name", result.Name);
            Assert.AreEqual("current_description", result.Description);
            Assert.AreEqual(13, result.Effort);
            Assert.IsTrue((DateTime.UtcNow - result.ModifiedTime).Duration().TotalMilliseconds < 1000);
        }

        [Test]
        public async Task DeleteTaskItemByIdShouldReturnFalseWhenItemDoesNotExist()
        {
            var result = await Subject.DeleteTaskItemById(1000).ConfigureAwait(false);

            Assert.IsFalse(result);
        }

        [Test]
        public async Task DeleteTaskItemByIdShouldReturnFalseWhenItemIsAlreadyDeleted()
        {
            var payload = new TaskItemCreationDto { Name = "name", Description = "description", Effort = 5 };
            await Subject.CreateTaskItem(payload).ConfigureAwait(false);
            await Subject.DeleteTaskItemById(1).ConfigureAwait(false);

            var result = await Subject.DeleteTaskItemById(1).ConfigureAwait(false);

            Assert.IsFalse(result);
        }

        [Test]
        public async Task DeleteTaskItemByIdShouldReturnTrueWhenSuccessfullyDeletedItem()
        {
            var payload = new TaskItemCreationDto { Name = "name", Description = "description", Effort = 5 };
            await Subject.CreateTaskItem(payload).ConfigureAwait(false);

            var result = await Subject.DeleteTaskItemById(1).ConfigureAwait(false);

            Assert.IsTrue(result);
        }

        [TearDown]
        public async Task TearDown()
        {
            await Context.Database.EnsureDeletedAsync().ConfigureAwait(false);
        }
    }
}
