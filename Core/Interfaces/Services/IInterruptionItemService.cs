using Core.Dtos;
using Core.Enums;
using Core.Models.WorkItem;
using System;
using System.Threading.Tasks;

namespace Core.Interfaces.Services
{
    public interface IInterruptionItemService
    {
        Task<ItemSummariesDto<InterruptionItemSummaryDto>> GetItemSummaries(DateTime start);
        Task<InterruptionItem> CreateItem(InterruptionItemBase item);
        Task<InterruptionItem> UpdateItem(InterruptionItem item, ResolveAction action = ResolveAction.None);
    }
}
