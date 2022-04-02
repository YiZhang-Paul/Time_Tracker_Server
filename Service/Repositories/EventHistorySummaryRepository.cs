using Core.DbContexts;
using Core.Enums;
using Core.Interfaces.Repositories;
using Core.Models.Event;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Service.Repositories
{
    public class EventHistorySummaryRepository : IEventHistorySummaryRepository
    {
        private TimeTrackerDbContext Context { get; }

        public EventHistorySummaryRepository(TimeTrackerDbContext context)
        {
            Context = context;
        }

        public async Task<EventHistorySummary> GetLastSummary(long userId, DateTime? end = null)
        {
            var endTime = end ?? DateTime.UtcNow;
            var query = Context.EventHistorySummary.OrderByDescending(_ => _.Timestamp).FirstOrDefaultAsync(_ => _.UserId == userId && _.Timestamp <= endTime);
            var summary = await query.ConfigureAwait(false);

            if (summary != null)
            {
                return summary;
            }

            return new EventHistorySummary
            {
                UserId = userId,
                Id = -1,
                ResourceId = -1,
                EventType = EventType.Idling,
                Timestamp = DateTime.MinValue.ToUniversalTime()
            };
        }

        public async Task<List<EventHistorySummary>> GetSummaries(long userId, DateTime start, DateTime end)
        {
            var query = Context.EventHistorySummary
                .Where(_ => _.UserId == userId && _.Timestamp >= start && _.Timestamp <= end)
                .OrderBy(_ => _.Timestamp);

            return await query.ToListAsync().ConfigureAwait(false);
        }
    }
}
