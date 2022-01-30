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

        public async Task<EventHistory> GetLastHistory(DateTime? end = null, bool isReadonly = false)
        {
            var endTime = end ?? DateTime.UtcNow;
            var query = isReadonly ? Context.EventHistory.AsNoTracking() : Context.EventHistory;

            return await query.OrderByDescending(_ => _.Timestamp).FirstOrDefaultAsync(_ => _.Timestamp <= endTime).ConfigureAwait(false);
        }

        public async Task<EventHistory> GetHistoryById(long id)
        {
            return await Context.EventHistory.FirstOrDefaultAsync(_ => _.Id == id).ConfigureAwait(false);
        }

        public async Task<List<EventHistory>> GetHistories(DateTime start, DateTime end)
        {
            return await Context.EventHistory
                .Where(_ => _.Timestamp >= start && _.Timestamp <= end)
                .OrderBy(_ => _.Timestamp)
                .ToListAsync()
                .ConfigureAwait(false);
        }

        public async Task<EventHistory> CreateHistory(EventHistory history)
        {
            history.Timestamp = DateTime.UtcNow;
            Context.EventHistory.Add(history);

            return await Context.SaveChangesAsync().ConfigureAwait(false) == 1 ? history : null;
        }
    }
}
