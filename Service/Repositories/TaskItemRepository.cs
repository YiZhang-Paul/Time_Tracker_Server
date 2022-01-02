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
            var items = Context.TaskItem
                .Where(_ => !_.IsDeleted)
                .Select(_ => new TaskItemSummaryDto { Id = _.Id, Name = _.Name, Effort = _.Effort });

            return await items.ToListAsync().ConfigureAwait(false);
        }

        public async Task<TaskItemSummaryDto> GetTaskItemSummaryById(long id)
        {
            return await Context.TaskItem
                .Where(_ => !_.IsDeleted)
                .Select(_ => new TaskItemSummaryDto { Id = _.Id, Name = _.Name, Effort = _.Effort })
                .FirstOrDefaultAsync(_ => _.Id == id)
                .ConfigureAwait(false);
        }

        public async Task<TaskItem> GetTaskItemById(long id, bool excludeDeleted = true)
        {
            if (excludeDeleted)
            {
                return await Context.TaskItem.FirstOrDefaultAsync(_ => _.Id == id && !_.IsDeleted).ConfigureAwait(false);
            }

            return await Context.TaskItem.FirstOrDefaultAsync(_ => _.Id == id).ConfigureAwait(false);
        }

        public async Task<TaskItem> CreateTaskItem(TaskItemCreationDto item)
        {
            var now = DateTime.UtcNow;

            var payload = new TaskItem
            {
                Name = item.Name,
                Description = item.Description,
                Effort = item.Effort,
                CreationTime = now,
                ModifiedTime = now
            };

            Context.TaskItem.Add(payload);

            return await Context.SaveChangesAsync().ConfigureAwait(false) == 1 ? payload : null;
        }

        public async Task<TaskItem> UpdateTaskItem(TaskItem item)
        {
            var existing = await GetTaskItemById(item.Id).ConfigureAwait(false);

            if (existing == null)
            {
                return null;
            }

            existing.Name = item.Name;
            existing.Description = item.Description;
            existing.Effort = item.Effort;
            existing.ModifiedTime = DateTime.UtcNow;

            return await Context.SaveChangesAsync().ConfigureAwait(false) == 1 ? existing : null;
        }

        public async Task<bool> DeleteTaskItemById(long id)
        {
            var existing = await GetTaskItemById(id).ConfigureAwait(false);

            if (existing == null)
            {
                return false;
            }

            existing.IsDeleted = true;
            existing.ModifiedTime = DateTime.UtcNow;

            return await Context.SaveChangesAsync().ConfigureAwait(false) == 1;
        }
    }
}
