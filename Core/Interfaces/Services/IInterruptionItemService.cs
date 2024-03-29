using Core.Dtos;
using Core.Enums;
using Core.Models.WorkItem;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Interfaces.Services
{
    public interface IInterruptionItemService
    {
        Task<List<InterruptionItemSummaryDto>> GetItemSummaries(long userId, string searchText);
        Task<ItemSummariesDto<InterruptionItemSummaryDto>> GetItemSummaries(long userId, DateTime start);
        Task<InterruptionItem> CreateItem(long userId, InterruptionItemBase item);
        Task<InterruptionItem> UpdateItem(long userId, InterruptionItem item, ResolveAction action = ResolveAction.None);
    }
}
