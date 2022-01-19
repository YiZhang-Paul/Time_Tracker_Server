using Core.Models.Event;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Interfaces.Repositories
{
    public interface IEventHistoryRepository
    {
        Task<EventHistory> GetLastHistory();
        Task<EventHistory> GetHistoryById(long id);
        Task<List<EventHistory>> GetHistories(DateTime start, DateTime end);
        Task<EventHistory> CreateHistory(EventHistory history);
    }
}
