using Core.Models.Event;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Interfaces.Repositories
{
    public interface IEventHistorySummaryRepository
    {
        Task<EventHistorySummary> GetLastSummary(DateTime? end = null);
        Task<List<EventHistorySummary>> GetSummaries(DateTime start, DateTime end);
    }
}
