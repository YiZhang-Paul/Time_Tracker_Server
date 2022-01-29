using Core.Enums;
using Core.Models.Task;
using System.Threading.Tasks;

namespace Core.Interfaces.Services
{
    public interface ITaskItemService
    {
        Task<TaskItem> UpdateItem(TaskItem item, ResolveAction action = ResolveAction.None);
    }
}
