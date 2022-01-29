using Core.Dtos;
using Core.Enums;
using Core.Models.Interruption;
using System;
using System.Threading.Tasks;

namespace Core.Interfaces.Services
{
    public interface IInterruptionItemService
    {
        Task<ItemSummariesDto> GetItemSummaries(DateTime start);
        Task<InterruptionItem> UpdateItem(InterruptionItem item, ResolveAction action = ResolveAction.None);
    }
}
