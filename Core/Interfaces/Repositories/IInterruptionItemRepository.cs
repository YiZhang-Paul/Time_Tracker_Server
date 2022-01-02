using Core.Dtos;
using Core.Models.Interruption;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Interfaces.Repositories
{
    public interface IInterruptionItemRepository
    {
        Task<List<InterruptionItemSummaryDto>> GetInterruptionItemSummaries();
        Task<InterruptionItemSummaryDto> GetInterruptionItemSummaryById(long id);
        Task<InterruptionItem> GetInterruptionItemById(long id, bool excludeDeleted = true);
        Task<InterruptionItem> CreateInterruptionItem(InterruptionItemCreationDto item);
        Task<InterruptionItem> UpdateInterruptionItem(InterruptionItem item);
        Task<bool> DeleteInterruptionItemById(long id);
    }
}
