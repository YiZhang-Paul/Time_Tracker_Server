using Core.Dtos;
using Core.Models.Task;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Interfaces.Repositories
{
    public interface ITaskItemRepository
    {
        Task<TaskItem> GetTaskItemById(long id);
        Task<List<TaskItemSummaryDto>> GetTaskItemSummaries();
        Task<TaskItem> CreateTaskItem(TaskItemCreationDto item);
    }
}
