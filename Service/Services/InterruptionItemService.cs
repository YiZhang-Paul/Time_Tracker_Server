using Core.Dtos;
using Core.Enums;
using Core.Interfaces.Repositories;
using Core.Interfaces.Services;
using Core.Models.WorkItem;
using System;
using System.Threading.Tasks;

namespace Service.Services
{
    public class InterruptionItemService : IInterruptionItemService
    {
        private IInterruptionItemRepository InterruptionItemRepository { get; }

        public InterruptionItemService(IInterruptionItemRepository interruptionItemRepository)
        {
            InterruptionItemRepository = interruptionItemRepository;
        }

        public async Task<ItemSummariesDto<InterruptionItemSummaryDto>> GetItemSummaries(DateTime start)
        {
            return new ItemSummariesDto<InterruptionItemSummaryDto>
            {
                Resolved = await InterruptionItemRepository.GetResolvedItemSummaries(start).ConfigureAwait(false),
                Unresolved = await InterruptionItemRepository.GetUnresolvedItemSummaries().ConfigureAwait(false)
            };
        }

        public async Task<InterruptionItem> UpdateItem(InterruptionItem item, ResolveAction action = ResolveAction.None)
        {
            if (string.IsNullOrWhiteSpace(item.Name))
            {
                throw new ArgumentException("Name must not be null or empty.");
            }

            if (action == ResolveAction.Resolve && item.ResolvedTime.HasValue)
            {
                throw new ArgumentException("Item is already resolved.");
            }

            if (action == ResolveAction.Unresolve && !item.ResolvedTime.HasValue)
            {
                throw new ArgumentException("Item is not resolved yet.");
            }

            if (action != ResolveAction.None)
            {
                item.ResolvedTime = action == ResolveAction.Resolve ? DateTime.UtcNow : (DateTime?)null;
            }

            return await InterruptionItemRepository.UpdateItem(item).ConfigureAwait(false);
        }
    }
}
