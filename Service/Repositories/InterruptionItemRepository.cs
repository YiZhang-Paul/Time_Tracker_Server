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
            var items = Context.InterruptionItem.Select(_ => new InterruptionItemSummaryDto { Id = _.Id, Name = _.Name, Priority = _.Priority });

            return await items.ToListAsync().ConfigureAwait(false);
        }

        public async Task<InterruptionItem> GetInterruptionItemById(long id)
        {
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
    }
}
