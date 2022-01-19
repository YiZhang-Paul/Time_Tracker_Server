using Core.DbContexts;
using Core.Interfaces.Repositories;
using Core.Models.Event;
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

        public async Task<EventHistory> GetLastHistory()
        {
            return await Context.EventHistory.OrderByDescending(_ => _.Id).FirstOrDefaultAsync().ConfigureAwait(false);
        }

        public async Task<EventHistory> GetHistoryById(long id)
        {
            return await Context.EventHistory.FirstOrDefaultAsync(_ => _.Id == id).ConfigureAwait(false);
        }

        public async Task<List<EventHistory>> GetHistories(DateTime start, DateTime end)
        {
            var startTime = start.ToUniversalTime();
            var endTime = end.ToUniversalTime();

            return await Context.EventHistory.Where(_ => _.Timestamp >= startTime && _.Timestamp <= endTime).ToListAsync().ConfigureAwait(false);
        }

        public async Task<EventHistory> CreateHistory(EventHistory history)
        {
            history.Timestamp = DateTime.UtcNow;
            Context.EventHistory.Add(history);

            return await Context.SaveChangesAsync().ConfigureAwait(false) == 1 ? history : null;
        }
    }
}
