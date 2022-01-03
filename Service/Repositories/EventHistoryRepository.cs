using Core.DbContexts;
using Core.Interfaces.Repositories;
using Core.Models.EventHistory;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
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
            return await Context.EventHistory.OrderByDescending(_ => _.Id).FirstOrDefaultAsync().ConfigureAwait(false);
        }

        public async Task<EventHistory> GetEventHistoryById(long id)
        {
            return await Context.EventHistory.FirstOrDefaultAsync(_ => _.Id == id).ConfigureAwait(false);
        }

        public async Task<List<EventHistory>> GetEventHistories(DateTime start, DateTime end)
        {
            return await Context.EventHistory.Where(_ => _.Timestamp >= start && _.Timestamp <= end).ToListAsync().ConfigureAwait(false);
        }

        public async Task<EventHistory> CreateEventHistory(EventHistory history)
        {
            history.Timestamp = DateTime.UtcNow;
            Context.EventHistory.Add(history);

            return await Context.SaveChangesAsync().ConfigureAwait(false) == 1 ? history : null;
        }
    }
}
