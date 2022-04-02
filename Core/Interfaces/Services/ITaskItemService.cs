using Core.Dtos;
using Core.Enums;
using Core.Models.WorkItem;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Interfaces.Services
{
    public interface ITaskItemService
    {
        Task<List<TaskItemSummaryDto>> GetItemSummaries(long userId, string searchText);
        Task<ItemSummariesDto<TaskItemSummaryDto>> GetItemSummaries(long userId, DateTime start);
        Task<TaskItem> CreateItem(TaskItemBase item);
        Task<TaskItem> UpdateItem(TaskItem item, ResolveAction action = ResolveAction.None);
    }
}
