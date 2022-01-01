using Core.DbContexts;
using Core.Interfaces.Repositories;
using Core.Models.ItemHistory;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace Service.Repositories
{
    public class ItemHistoryRepository : IItemHistoryRepository
    {
        private TimeTrackerDbContext Context { get; }

        public ItemHistoryRepository(TimeTrackerDbContext context)
        {
            Context = context;
        }

        public async Task<ItemHistory> GetLastItemHistory()
        {
            return await Context.ItemHistory.LastOrDefaultAsync().ConfigureAwait(false);
        }

        public async Task<ItemHistory> CreateItemHistory(ItemHistory history)
        {
            history.Timestamp = DateTime.UtcNow;
            Context.ItemHistory.Add(history);

            return await Context.SaveChangesAsync().ConfigureAwait(false) == 1 ? history : null;
        }
    }
}
