using Core.Models.EventHistory;
using System;
using System.Threading.Tasks;

namespace Core.Interfaces.Services
{
    public interface IEventHistoryService
    {
        Task<EventTimeDistribution> GetTimeDistribution(DateTime start);
        Task<bool> StartIdlingSession();
        Task<bool> StartInterruptionItem(long id);
        Task<bool> StartTaskItem(long id);
    }
}
