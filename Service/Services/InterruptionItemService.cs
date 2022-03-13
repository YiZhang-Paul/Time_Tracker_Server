using Core.Dtos;
using Core.Enums;
using Core.Interfaces.Services;
using Core.Interfaces.UnitOfWorks;
using Core.Models.WorkItem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Service.Services
{
    public class InterruptionItemService : IInterruptionItemService
    {
        private IWorkItemUnitOfWork WorkItemUnitOfWork { get; }

        public InterruptionItemService(IWorkItemUnitOfWork workItemUnitOfWork)
        {
            WorkItemUnitOfWork = workItemUnitOfWork;
        }

        public async Task<List<InterruptionItemSummaryDto>> GetItemSummaries(string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
            {
                throw new ArgumentException("Search text must not be empty.");
            }

            return await WorkItemUnitOfWork.InterruptionItem.GetItemSummaries(searchText.Trim()).ConfigureAwait(false);
        }

        public async Task<ItemSummariesDto<InterruptionItemSummaryDto>> GetItemSummaries(DateTime start)
        {
            return new ItemSummariesDto<InterruptionItemSummaryDto>
            {
                Resolved = await WorkItemUnitOfWork.InterruptionItem.GetResolvedItemSummaries(start).ConfigureAwait(false),
                Unresolved = await WorkItemUnitOfWork.InterruptionItem.GetUnresolvedItemSummaries().ConfigureAwait(false)
            };
        }

        public async Task<InterruptionItem> CreateItem(InterruptionItemBase item)
        {
            ValidateItem(item);
            var created = WorkItemUnitOfWork.InterruptionItem.CreateItem(item);

            return await WorkItemUnitOfWork.Save().ConfigureAwait(false) > 0 ? created : null;
        }

        public async Task<InterruptionItem> UpdateItem(InterruptionItem item, ResolveAction action = ResolveAction.None)
        {
            ValidateItem(item);

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
                item.ResolvedTime = action == ResolveAction.Resolve ? DateTime.UtcNow : null;
            }

            var updated = await WorkItemUnitOfWork.InterruptionItem.UpdateItem(item).ConfigureAwait(false);

            return updated != null && await WorkItemUnitOfWork.Save().ConfigureAwait(false) > 0 ? updated : null;
        }

        private void ValidateItem(InterruptionItemBase item)
        {
            if (string.IsNullOrWhiteSpace(item.Name))
            {
                throw new ArgumentException("Name must not be null or empty.");
            }

            if (item.Checklists.Any(_ => string.IsNullOrWhiteSpace(_.Description)))
            {
                throw new ArgumentException("Checklist description must not be null or empty.");
            }
        }
    }
}
