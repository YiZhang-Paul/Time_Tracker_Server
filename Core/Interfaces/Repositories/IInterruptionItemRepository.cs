using Core.Dtos;
using Core.Models.Interruption;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Interfaces.Repositories
{
    public interface IInterruptionItemRepository
    {
        Task<List<InterruptionItemSummaryDto>> GetItemSummaries();
        Task<InterruptionItem> GetItemById(long id, bool excludeDeleted = true);
        Task<InterruptionItem> CreateItem(InterruptionItemCreationDto item);
        Task<InterruptionItem> UpdateItem(InterruptionItem item);
        Task<bool> DeleteItemById(long id);
    }
}
