using Core.Dtos;
using System.Threading.Tasks;

namespace Core.Interfaces.Repositories
{
    public interface IEventTrackingService
    {
        Task<bool> StartIdlingSession(long userId);
        Task<bool> StartInterruptionItem(long userId, long id);
        Task<bool> StartTaskItem(long userId, long id);
        Task<bool> StartBreakSession(long userId, int duration);
        Task<bool> SkipBreakSession(long userId);
        Task<bool> UpdateTimeRange(long userId, EventTimeRangeDto range);
    }
}
