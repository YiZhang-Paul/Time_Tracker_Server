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
    public class TaskItemService : ITaskItemService
    {
        private IWorkItemUnitOfWork WorkItemUnitOfWork { get; }

        public TaskItemService(IWorkItemUnitOfWork workItemUnitOfWork)
        {
            WorkItemUnitOfWork = workItemUnitOfWork;
        }

        public async Task<List<TaskItemSummaryDto>> GetItemSummaries(long userId, string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
            {
                throw new ArgumentException("Search text must not be empty.");
            }

            return await WorkItemUnitOfWork.TaskItem.GetItemSummaries(userId, searchText.Trim()).ConfigureAwait(false);
        }

        public async Task<ItemSummariesDto<TaskItemSummaryDto>> GetItemSummaries(long userId, DateTime start)
        {
            return new ItemSummariesDto<TaskItemSummaryDto>
            {
                Resolved = await WorkItemUnitOfWork.TaskItem.GetResolvedItemSummaries(userId, start).ConfigureAwait(false),
                Unresolved = await WorkItemUnitOfWork.TaskItem.GetUnresolvedItemSummaries(userId).ConfigureAwait(false)
            };
        }

        public async Task<TaskItem> CreateItem(long userId, TaskItemBase item)
        {
            ValidateItem(item);
            var created = WorkItemUnitOfWork.TaskItem.CreateItem(userId, item);

            return await WorkItemUnitOfWork.Save().ConfigureAwait(false) ? created : null;
        }

        public async Task<TaskItem> UpdateItem(long userId, TaskItem item, ResolveAction action = ResolveAction.None)
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

            var updated = await WorkItemUnitOfWork.TaskItem.UpdateItem(item).ConfigureAwait(false);

            return updated != null && await WorkItemUnitOfWork.Save().ConfigureAwait(false) ? updated : null;
        }

        private void ValidateItem(TaskItemBase item)
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
