using Core.Dtos;
using Core.Models.WorkItem;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Interfaces.Repositories
{
    public interface ITaskItemRepository
    {
        Task<List<TaskItemSummaryDto>> GetItemSummaries(long userId, string searchText);
        Task<List<TaskItemSummaryDto>> GetResolvedItemSummaries(long userId, DateTime start);
        Task<List<TaskItemSummaryDto>> GetUnresolvedItemSummaries(long userId);
        Task<TaskItem> GetItemById(long userId, long id, bool excludeDeleted = true);
        TaskItem CreateItem(long userId, TaskItemBase item);
        Task<TaskItem> UpdateItem(TaskItem item);
        Task<bool> DeleteItemById(long userId, long id);
        Task<bool> DeleteItem(TaskItem item);
    }
}
