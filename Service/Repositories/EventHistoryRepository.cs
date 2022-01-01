using Core.DbContexts;
using Core.Interfaces.Repositories;
using Core.Models.EventHistory;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace Service.Repositories
{
    public class EventHistoryRepository : IEventHistoryRepository
    {
        private TimeTrackerDbContext Context { get; }

        public EventHistoryRepository(TimeTrackerDbContext context)
        {
            Context = context;
        }

        public async Task<EventHistory> GetLastEventHistory()
        {
            return await Context.EventHistory.LastOrDefaultAsync().ConfigureAwait(false);
        }

        public async Task<EventHistory> CreateEventHistory(EventHistory history)
        {
            history.Timestamp = DateTime.UtcNow;
            Context.EventHistory.Add(history);

            return await Context.SaveChangesAsync().ConfigureAwait(false) == 1 ? history : null;
        }
    }
}
