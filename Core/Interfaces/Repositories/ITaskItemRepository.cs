using Core.Dtos;
using Core.Models.Task;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Interfaces.Repositories
{
    public interface ITaskItemRepository
    {
        Task<List<TaskItemSummaryDto>> GetItemSummaries();
        Task<TaskItem> GetItemById(long id, bool excludeDeleted = true);
        Task<TaskItem> CreateItem(TaskItemCreationDto item);
        Task<TaskItem> UpdateItem(TaskItem item);
        Task<bool> DeleteItemById(long id);
    }
}
