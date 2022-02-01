using Core.DbContexts;
using Core.Interfaces.Repositories;
using Core.Models.Event;
using Microsoft.EntityFrameworkCore;
using Service.Extensions;
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

        public async Task<EventHistorySummary> GetLastSummary(DateTime? end = null)
        {
            var endTime = end ?? DateTime.UtcNow;

            return await Context.EventHistorySummary.OrderByDescending(_ => _.Timestamp).FirstOrDefaultAsync(_ => _.Timestamp <= endTime).ConfigureAwait(false);
        }

        public async Task<List<EventHistorySummary>> GetSummaries(DateTime start, DateTime end)
        {
            var query = Context.EventHistorySummary.Where(_ => _.Timestamp >= start && _.Timestamp <= end).OrderBy(_ => _.Timestamp);
            var summaries = await query.ToListAsync().ConfigureAwait(false);
            summaries.ForEach(_ => _.Timestamp = _.Timestamp.SpecifyKindUtc());

            return summaries;
        }
    }
}
