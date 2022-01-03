using Core.Models.EventHistory;
using System.Threading.Tasks;

namespace Core.Interfaces.Services
{
    public interface IEventHistoryService
    {
        Task<EventTimeDistribution> GetCurrentTimeDistribution();
        Task<bool> StartIdlingSession();
        Task<bool> StartInterruptionItem(long id);
        Task<bool> StartTaskItem(long id);
    }
}
