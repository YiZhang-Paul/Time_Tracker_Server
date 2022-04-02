using Core.Dtos;
using Core.Models.WorkItem;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Interfaces.Repositories
{
    public interface IInterruptionItemRepository
    {
        Task<List<InterruptionItemSummaryDto>> GetItemSummaries(long userId, string searchText);
        Task<List<InterruptionItemSummaryDto>> GetResolvedItemSummaries(long userId, DateTime start);
        Task<List<InterruptionItemSummaryDto>> GetUnresolvedItemSummaries(long userId);
        Task<InterruptionItem> GetItemById(long userId, long id, bool excludeDeleted = true);
        InterruptionItem CreateItem(InterruptionItemBase item);
        Task<InterruptionItem> UpdateItem(InterruptionItem item);
        Task<bool> DeleteItemById(long userId, long id);
        Task<bool> DeleteItem(InterruptionItem item);
    }
}
