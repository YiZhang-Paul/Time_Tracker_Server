using Core.Models.Event;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Interfaces.Repositories
{
    public interface IEventHistoryRepository
    {
        Task<EventHistory> GetLastHistory(long userId, DateTime? end = null, bool isReadonly = false);
        Task<EventHistory> GetNextHistory(long userId, DateTime start, bool isReadonly = false);
        Task<List<EventHistory>> GetHistories(long userId, DateTime start, DateTime end);
        EventHistory CreateHistory(long userId, EventHistory history);
        void DeleteHistory(EventHistory history);
        void DeleteHistories(List<EventHistory> histories);
    }
}
