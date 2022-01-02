using System.Threading.Tasks;

namespace Core.Interfaces.Services
{
    public interface IEventHistoryService
    {
        Task<bool> StartIdlingSession();
        Task<bool> StartInterruptionItem(long id);
        Task<bool> StartTaskItem(long id);
    }
}
