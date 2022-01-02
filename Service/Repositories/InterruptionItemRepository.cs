using Core.DbContexts;
using Core.Dtos;
using Core.Interfaces.Repositories;
using Core.Models.Interruption;
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

        public async Task<List<InterruptionItemSummaryDto>> GetInterruptionItemSummaries()
        {
            var items = Context.InterruptionItem
                .Where(_ => !_.IsDeleted)
                .Select(_ => new InterruptionItemSummaryDto { Id = _.Id, Name = _.Name, Priority = _.Priority });

            return await items.ToListAsync().ConfigureAwait(false);
        }

        public async Task<InterruptionItemSummaryDto> GetInterruptionItemSummaryById(long id)
        {
            return await Context.InterruptionItem
                .Where(_ => !_.IsDeleted)
                .Select(_ => new InterruptionItemSummaryDto { Id = _.Id, Name = _.Name, Priority = _.Priority })
                .FirstOrDefaultAsync(_ => _.Id == id)
                .ConfigureAwait(false);
        }

        public async Task<InterruptionItem> GetInterruptionItemById(long id, bool excludeDeleted = true)
        {
            if (excludeDeleted)
            {
                return await Context.InterruptionItem.FirstOrDefaultAsync(_ => _.Id == id && !_.IsDeleted).ConfigureAwait(false);
            }

            return await Context.InterruptionItem.FirstOrDefaultAsync(_ => _.Id == id).ConfigureAwait(false);
        }

        public async Task<InterruptionItem> CreateInterruptionItem(InterruptionItemCreationDto item)
        {
            var now = DateTime.UtcNow;

            var payload = new InterruptionItem
            {
                Name = item.Name,
                Description = item.Description,
                Priority = item.Priority,
                CreationTime = now,
                ModifiedTime = now
            };

            Context.InterruptionItem.Add(payload);

            return await Context.SaveChangesAsync().ConfigureAwait(false) == 1 ? payload : null;
        }

        public async Task<InterruptionItem> UpdateInterruptionItem(InterruptionItem item)
        {
            var existing = await GetInterruptionItemById(item.Id).ConfigureAwait(false);

            if (existing == null)
            {
                return null;
            }

            existing.Name = item.Name;
            existing.Description = item.Description;
            existing.Priority = item.Priority;
            existing.ModifiedTime = DateTime.UtcNow;

            return await Context.SaveChangesAsync().ConfigureAwait(false) == 1 ? existing : null;
        }

        public async Task<bool> DeleteInterruptionItemById(long id)
        {
            var existing = await GetInterruptionItemById(id).ConfigureAwait(false);

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
