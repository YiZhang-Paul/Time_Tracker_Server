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
    public class EventHistorySummaryRepository : IEventHistorySummaryRepository
    {
        private TimeTrackerDbContext Context { get; }

        public EventHistorySummaryRepository(TimeTrackerDbContext context)
        {
            Context = context;
        }

        public async Task<EventHistorySummary> GetSummaryById(long id)
        {
            return await Context.EventHistorySummary.FirstOrDefaultAsync(_ => _.Id == id).ConfigureAwait(false);
        }

        public async Task<List<EventHistorySummary>> GetSummaries(DateTime start, DateTime end)
        {
            return await Context.EventHistorySummary.Where(_ => _.Timestamp >= start && _.Timestamp <= end).ToListAsync().ConfigureAwait(false);
        }
    }
}
