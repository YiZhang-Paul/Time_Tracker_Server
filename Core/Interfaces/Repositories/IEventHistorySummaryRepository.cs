using Core.Models.Event;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Interfaces.Repositories
{
    public interface IEventHistorySummaryRepository
    {
        Task<EventHistorySummary> GetLastSummary(long userId, DateTime? end = null);
        Task<List<EventHistorySummary>> GetSummaries(long userId, DateTime start, DateTime end);
    }
}
