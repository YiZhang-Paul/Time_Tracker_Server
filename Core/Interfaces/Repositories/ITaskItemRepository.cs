using Core.Dtos;
using Core.Models.WorkItem;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Interfaces.Repositories
{
    public interface ITaskItemRepository
    {
        Task<List<TaskItemSummaryDto>> GetResolvedItemSummaries(DateTime start);
        Task<List<TaskItemSummaryDto>> GetUnresolvedItemSummaries();
        Task<TaskItem> GetItemById(long id, bool excludeDeleted = true);
        Task<TaskItem> CreateItem(TaskItemBase item);
        Task<TaskItem> UpdateItem(TaskItem item);
        Task<bool> DeleteItemById(long id);
    }
}
