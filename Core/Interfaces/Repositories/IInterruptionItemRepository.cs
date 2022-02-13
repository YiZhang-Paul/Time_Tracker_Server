using Core.Dtos;
using Core.Models.WorkItem;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Interfaces.Repositories
{
    public interface IInterruptionItemRepository
    {
        Task<List<InterruptionItemSummaryDto>> GetResolvedItemSummaries(DateTime start);
        Task<List<InterruptionItemSummaryDto>> GetUnresolvedItemSummaries();
        Task<InterruptionItem> GetItemById(long id, bool excludeDeleted = true);
        Task<InterruptionItem> CreateItem(InterruptionItemCreationDto item);
        Task<InterruptionItem> UpdateItem(InterruptionItem item);
        Task<bool> DeleteItemById(long id);
    }
}
