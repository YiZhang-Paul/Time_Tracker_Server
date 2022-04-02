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

        public async Task<List<InterruptionItemSummaryDto>> GetItemSummaries(long userId, string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
            {
                throw new ArgumentException("Search text must not be empty.");
            }

            return await WorkItemUnitOfWork.InterruptionItem.GetItemSummaries(userId, searchText.Trim()).ConfigureAwait(false);
        }

        public async Task<ItemSummariesDto<InterruptionItemSummaryDto>> GetItemSummaries(long userId, DateTime start)
        {
            return new ItemSummariesDto<InterruptionItemSummaryDto>
            {
                Resolved = await WorkItemUnitOfWork.InterruptionItem.GetResolvedItemSummaries(userId, start).ConfigureAwait(false),
                Unresolved = await WorkItemUnitOfWork.InterruptionItem.GetUnresolvedItemSummaries(userId).ConfigureAwait(false)
            };
        }

        public async Task<InterruptionItem> CreateItem(long userId, InterruptionItemBase item)
        {
            ValidateItem(item);
            var created = WorkItemUnitOfWork.InterruptionItem.CreateItem(userId, item);

            return await WorkItemUnitOfWork.Save().ConfigureAwait(false) ? created : null;
        }

        public async Task<InterruptionItem> UpdateItem(long userId, InterruptionItem item, ResolveAction action = ResolveAction.None)
        {
            item.UserId = userId;
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

            return updated != null && await WorkItemUnitOfWork.Save().ConfigureAwait(false) ? updated : null;
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
