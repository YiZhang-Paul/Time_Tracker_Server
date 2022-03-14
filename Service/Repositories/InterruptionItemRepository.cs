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
    public class InterruptionItemRepository : IInterruptionItemRepository
    {
        private TimeTrackerDbContext Context { get; }

        public InterruptionItemRepository(TimeTrackerDbContext context)
        {
            Context = context;
        }

        public async Task<List<InterruptionItemSummaryDto>> GetItemSummaries(string searchText)
        {
            var query = Context.InterruptionItem
                .Where(_ => _.Name.ToLower().Contains(searchText.ToLower()))
                .Select(_ => InterruptionItemSummaryDto.Convert(_));

            return await query.ToListAsync().ConfigureAwait(false);
        }

        public async Task<List<InterruptionItemSummaryDto>> GetResolvedItemSummaries(DateTime start)
        {
            var query = Context.InterruptionItem
                .Where(_ => !_.IsDeleted && _.ResolvedTime >= start)
                .Include(_ => _.Checklists)
                .Select(_ => InterruptionItemSummaryDto.Convert(_));

            return await query.ToListAsync().ConfigureAwait(false);
        }

        public async Task<List<InterruptionItemSummaryDto>> GetUnresolvedItemSummaries()
        {
            var query = Context.InterruptionItem
                .Where(_ => !_.IsDeleted && _.ResolvedTime == null)
                .Include(_ => _.Checklists)
                .Select(_ => InterruptionItemSummaryDto.Convert(_));

            return await query.ToListAsync().ConfigureAwait(false);
        }

        public async Task<InterruptionItem> GetItemById(long id, bool excludeDeleted = true)
        {
            var query = Context.InterruptionItem.Include(_ => _.Checklists.OrderBy(entry => entry.Rank));

            if (excludeDeleted)
            {
                return await query.FirstOrDefaultAsync(_ => _.Id == id && !_.IsDeleted).ConfigureAwait(false);
            }

            return await query.FirstOrDefaultAsync(_ => _.Id == id).ConfigureAwait(false);
        }

        public InterruptionItem CreateItem(InterruptionItemBase item)
        {
            var now = DateTime.UtcNow;

            var payload = new InterruptionItem
            {
                Name = item.Name,
                Description = item.Description,
                Priority = item.Priority,
                Checklists = item.Checklists,
                CreationTime = now,
                ModifiedTime = now
            };

            return Context.InterruptionItem.Add(payload).Entity;
        }

        public async Task<InterruptionItem> UpdateItem(InterruptionItem item)
        {
            if (await Context.InterruptionItem.AllAsync(_ => _.Id != item.Id).ConfigureAwait(false))
            {
                return null;
            }

            item.ModifiedTime = DateTime.UtcNow;

            return Context.InterruptionItem.Update(item).Entity;
        }

        public async Task<bool> DeleteItemById(long id)
        {
            var item = await GetItemById(id).ConfigureAwait(false);

            if (item == null)
            {
                return false;
            }

            item.IsDeleted = true;
            item.ModifiedTime = DateTime.UtcNow;

            return true;
        }

        public async Task<bool> DeleteItem(InterruptionItem item)
        {
            if (await Context.InterruptionItem.AllAsync(_ => _.Id != item.Id).ConfigureAwait(false))
            {
                return false;
            }

            item.IsDeleted = true;
            item.ModifiedTime = DateTime.UtcNow;
            Context.InterruptionItem.Update(item);

            return true;
        }
    }
}
