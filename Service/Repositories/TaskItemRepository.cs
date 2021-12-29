using Core.DbContexts;
using Core.Dtos;
using Core.Interfaces.Repositories;
using Core.Models.Task;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Service.Repositories
{
    public class TaskItemRepository : ITaskItemRepository
    {
        private TimeTrackerDbContext Context { get; }

        public TaskItemRepository(TimeTrackerDbContext context)
        {
            Context = context;
        }

        public async Task<List<TaskItemSummaryDto>> GetTaskItemSummaries()
        {
            var items = Context.TaskItem.Select(_ => new TaskItemSummaryDto { Id = _.Id, Name = _.Name });

            return await items.ToListAsync().ConfigureAwait(false);
        }

        public async Task<TaskItem> GetTaskItemById(long id)
        {
            return await Context.TaskItem.FirstOrDefaultAsync(_ => _.Id == id).ConfigureAwait(false);
        }

        public async Task<TaskItem> CreateTaskItem(TaskItemCreationDto item)
        {
            var now = DateTime.UtcNow;

            var payload = new TaskItem
            {
                Name = item.Name,
                Description = item.Description,
                CreationTime = now,
                ModifiedTime = now
            };

            Context.TaskItem.Add(payload);

            return await Context.SaveChangesAsync().ConfigureAwait(false) == 1 ? payload : null;
        }

        public async Task<bool> DeleteTaskItemById(long id)
        {
            Context.TaskItem.Remove(new TaskItem { Id = id });

            return await Context.SaveChangesAsync().ConfigureAwait(false) == 1;
        }
    }
}
