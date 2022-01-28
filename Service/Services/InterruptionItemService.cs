using Core.Enums;
using Core.Interfaces.Repositories;
using Core.Interfaces.Services;
using Core.Models.Interruption;
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

        public async Task<InterruptionItem> UpdateItem(InterruptionItem item, ResolveAction action = ResolveAction.None)
        {
            if (string.IsNullOrWhiteSpace(item.Name))
            {
                throw new ArgumentException("Name must not be null or empty.");
            }

            var existing = await InterruptionItemRepository.GetItemById(item.Id).ConfigureAwait(false);

            if (existing == null)
            {
                return null;
            }

            return await InterruptionItemRepository.UpdateItem(SetResolvedTime(existing, action)).ConfigureAwait(false);
        }

        private InterruptionItem SetResolvedTime(InterruptionItem item, ResolveAction action)
        {
            if (action == ResolveAction.None)
            {
                return item;
            }

            if (action == ResolveAction.Resolve && item.ResolvedTime.HasValue)
            {
                throw new ArgumentException("Item is already resolved.");
            }

            if (action == ResolveAction.Unresolve && !item.ResolvedTime.HasValue)
            {
                throw new ArgumentException("Item is not resolved yet.");
            }

            item.ResolvedTime = action == ResolveAction.Resolve ? DateTime.UtcNow : (DateTime?)null;

            return item;
        }
    }
}
