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
        Task<List<InterruptionItemSummaryDto>> GetItemSummaries(string searchText);
        Task<ItemSummariesDto<InterruptionItemSummaryDto>> GetItemSummaries(DateTime start);
        Task<InterruptionItem> CreateItem(InterruptionItemBase item);
        Task<InterruptionItem> UpdateItem(InterruptionItem item, ResolveAction action = ResolveAction.None);
    }
}
