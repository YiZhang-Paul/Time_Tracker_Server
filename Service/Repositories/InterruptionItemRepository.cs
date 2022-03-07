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

        public async Task<InterruptionItem> CreateItem(InterruptionItemBase item)
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

            Context.InterruptionItem.Add(payload);

            return await Context.SaveChangesAsync().ConfigureAwait(false) > 0 ? payload : null;
        }

        public async Task<InterruptionItem> UpdateItem(InterruptionItem item)
        {
            var existing = await GetItemById(item.Id).ConfigureAwait(false);

            if (existing == null)
            {
                return null;
            }

            existing.Name = item.Name;
            existing.Description = item.Description;
            existing.Priority = item.Priority;
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
