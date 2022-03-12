using Core.Dtos;
using System.Threading.Tasks;

namespace Core.Interfaces.Repositories
{
    public interface IEventTrackingService
    {
        Task<bool> StartIdlingSession();
        Task<bool> StartInterruptionItem(long id);
        Task<bool> StartTaskItem(long id);
        Task<bool> StartBreakSession(int duration);
        Task<bool> SkipBreakSession();
        Task<bool> UpdateTimeRange(EventTimeRangeDto range);
    }
}
