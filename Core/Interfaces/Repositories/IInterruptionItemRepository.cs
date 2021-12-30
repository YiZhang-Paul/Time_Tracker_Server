using Core.Dtos;
using Core.Models.Interruption;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Interfaces.Repositories
{
    public interface IInterruptionItemRepository
    {
        Task<List<InterruptionItemSummaryDto>> GetInterruptionItemSummaries();
        Task<InterruptionItem> CreateInterruptionItem(InterruptionItemCreationDto item);
    }
}
