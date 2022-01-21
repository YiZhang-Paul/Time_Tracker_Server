using Core.Models.Event;
using System;
using System.Threading.Tasks;

namespace Core.Interfaces.Services
{
    public interface IEventService
    {
        Task<OngoingEventTimeSummary> GetOngoingTimeSummary(DateTime start);
        Task<bool> StartIdlingSession();
        Task<bool> StartInterruptionItem(long id);
        Task<bool> StartTaskItem(long id);
        Task<bool> StartBreakSession(int duration);
        Task<bool> SkipBreakSession();
    }
}
