using Core.Models.Interruption;
using Core.Models.Task;
using System.Threading.Tasks;

namespace Core.Interfaces.Services
{
    public interface IEventHistoryService
    {
        Task<bool> StartIdlingSession();
        Task<bool> StartInterruptionItem(InterruptionItem item);
        Task<bool> StartTaskItem(TaskItem item);
    }
}
