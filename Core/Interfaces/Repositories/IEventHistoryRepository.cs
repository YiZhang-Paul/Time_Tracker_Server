using Core.Models.Event;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Interfaces.Repositories
{
    public interface IEventHistoryRepository
    {
        Task<EventHistory> GetLastHistory(DateTime? end = null, bool isReadonly = false);
        Task<EventHistory> GetNextHistory(DateTime start, bool isReadonly = false);
        Task<EventHistory> GetHistoryById(long id);
        Task<List<EventHistory>> GetHistories(DateTime start, DateTime end);
        EventHistory CreateHistory(EventHistory history);
        void DeleteHistory(EventHistory history);
        void DeleteHistories(List<EventHistory> histories);
    }
}
