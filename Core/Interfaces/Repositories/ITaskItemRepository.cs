using Core.Dtos;
using Core.Models.Task;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Interfaces.Repositories
{
    public interface ITaskItemRepository
    {
        Task<List<TaskItemSummaryDto>> GetTaskItemSummaries();
        Task<TaskItem> GetTaskItemById(long id);
        Task<TaskItem> CreateTaskItem(TaskItemCreationDto item);
        Task<TaskItem> UpdateTaskItem(TaskItem item);
        Task<bool> DeleteTaskItemById(long id);
    }
}
