using Core.DbContexts;
using Core.Dtos;
using Core.Interfaces.Repositories;
using Core.Models.WorkItem;
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

        public async Task<List<TaskItemSummaryDto>> GetItemSummaries(string searchText)
        {
            var query = Context.TaskItem
                .Where(_ => _.Name.ToLower().Contains(searchText.ToLower()))
                .Select(_ => TaskItemSummaryDto.Convert(_));

            return await query.ToListAsync().ConfigureAwait(false);
        }

        public async Task<List<TaskItemSummaryDto>> GetResolvedItemSummaries(DateTime start)
        {
            var query = Context.TaskItem
                .Where(_ => !_.IsDeleted && _.ResolvedTime >= start)
                .Include(_ => _.Checklists)
                .Select(_ => TaskItemSummaryDto.Convert(_));

            return await query.ToListAsync().ConfigureAwait(false);
        }

        public async Task<List<TaskItemSummaryDto>> GetUnresolvedItemSummaries()
        {
            var query = Context.TaskItem
                .Where(_ => !_.IsDeleted && _.ResolvedTime == null)
                .Include(_ => _.Checklists)
                .Select(_ => TaskItemSummaryDto.Convert(_));

            return await query.ToListAsync().ConfigureAwait(false);
        }

        public async Task<TaskItem> GetItemById(long id, bool excludeDeleted = true)
        {
            var query = Context.TaskItem.Include(_ => _.Checklists.OrderBy(entry => entry.Rank));

            if (excludeDeleted)
            {
                return await query.FirstOrDefaultAsync(_ => _.Id == id && !_.IsDeleted).ConfigureAwait(false);
            }

            return await query.FirstOrDefaultAsync(_ => _.Id == id).ConfigureAwait(false);
        }

        public async Task<TaskItem> CreateItem(TaskItemBase item)
        {
            var now = DateTime.UtcNow;

            var payload = new TaskItem
            {
                Name = item.Name,
                Description = item.Description,
                Effort = item.Effort,
                Checklists = item.Checklists,
                CreationTime = now,
                ModifiedTime = now
            };

            Context.TaskItem.Add(payload);

            return await Context.SaveChangesAsync().ConfigureAwait(false) > 0 ? payload : null;
        }

        public async Task<TaskItem> UpdateItem(TaskItem item)
        {
            var existing = await GetItemById(item.Id).ConfigureAwait(false);

            if (existing == null)
            {
                return null;
            }

            existing.Name = item.Name;
            existing.Description = item.Description;
            existing.Effort = item.Effort;
            existing.Checklists = item.Checklists;
            existing.ResolvedTime = item.ResolvedTime;
            existing.ModifiedTime = DateTime.UtcNow;

            return await Context.SaveChangesAsync().ConfigureAwait(false) > 0 ? existing : null;
        }

        public async Task<bool> DeleteItemById(long id)
        {
            var existing = await GetItemById(id).ConfigureAwait(false);

            if (existing == null)
            {
                return false;
            }

            existing.IsDeleted = true;
            existing.ModifiedTime = DateTime.UtcNow;

            return await Context.SaveChangesAsync().ConfigureAwait(false) > 0;
        }
    }
}
