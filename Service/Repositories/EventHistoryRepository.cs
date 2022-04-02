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

        public async Task<EventHistory> GetLastHistory(long userId, DateTime? end = null, bool isReadonly = false)
        {
            var endTime = end ?? DateTime.UtcNow;
            var query = isReadonly ? Context.EventHistory.AsNoTracking() : Context.EventHistory;

            return await query.OrderByDescending(_ => _.Timestamp).FirstOrDefaultAsync(_ => _.UserId == userId && _.Timestamp <= endTime).ConfigureAwait(false);
        }

        public async Task<EventHistory> GetNextHistory(long userId, DateTime start, bool isReadonly = false)
        {
            var query = isReadonly ? Context.EventHistory.AsNoTracking() : Context.EventHistory;

            return await query.OrderBy(_ => _.Timestamp).FirstOrDefaultAsync(_ => _.UserId == userId && _.Timestamp >= start).ConfigureAwait(false);
        }

        public async Task<List<EventHistory>> GetHistories(long userId, DateTime start, DateTime end)
        {
            return await Context.EventHistory
                .Where(_ => _.UserId == userId && _.Timestamp >= start && _.Timestamp <= end)
                .OrderBy(_ => _.Timestamp)
                .ToListAsync()
                .ConfigureAwait(false);
        }

        public EventHistory CreateHistory(long userId, EventHistory history)
        {
            if (history.Timestamp == default)
            {
                history.Timestamp = DateTime.UtcNow;
            }

            history.UserId = userId;

            return Context.EventHistory.Add(history).Entity;
        }

        public void DeleteHistory(EventHistory history)
        {
            Context.EventHistory.Remove(history);
        }

        public void DeleteHistories(List<EventHistory> histories)
        {
            Context.EventHistory.RemoveRange(histories);
        }
    }
}
