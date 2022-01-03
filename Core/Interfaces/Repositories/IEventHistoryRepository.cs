using Core.Models.EventHistory;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Interfaces.Repositories
{
    public interface IEventHistoryRepository
    {
        Task<EventHistory> GetLastEventHistory();
        Task<EventHistory> GetEventHistoryById(long id);
        Task<List<EventHistory>> GetEventHistories(DateTime start, DateTime end);
        Task<EventHistory> CreateEventHistory(EventHistory history);
    }
}
