using Core.Dtos;
using Core.Enums;
using Core.Interfaces.Repositories;
using Core.Interfaces.Services;
using Core.Models.WorkItem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Service.Services
{
    public class TaskItemService : ITaskItemService
    {
        private ITaskItemRepository TaskItemRepository { get; }

        public TaskItemService(ITaskItemRepository taskItemRepository)
        {
            TaskItemRepository = taskItemRepository;
        }

        public async Task<List<TaskItemSummaryDto>> GetItemSummaries(string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
            {
                throw new ArgumentException("Search text must not be empty.");
            }

            return await TaskItemRepository.GetItemSummaries(searchText.Trim()).ConfigureAwait(false);
        }

        public async Task<ItemSummariesDto<TaskItemSummaryDto>> GetItemSummaries(DateTime start)
        {
            return new ItemSummariesDto<TaskItemSummaryDto>
            {
                Resolved = await TaskItemRepository.GetResolvedItemSummaries(start).ConfigureAwait(false),
                Unresolved = await TaskItemRepository.GetUnresolvedItemSummaries().ConfigureAwait(false)
            };
        }

        public async Task<TaskItem> CreateItem(TaskItemBase item)
        {
            ValidateItem(item);

            return await TaskItemRepository.CreateItem(item).ConfigureAwait(false);
        }

        public async Task<TaskItem> UpdateItem(TaskItem item, ResolveAction action = ResolveAction.None)
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
                item.ResolvedTime = action == ResolveAction.Resolve ? DateTime.UtcNow : (DateTime?)null;
            }

            return await TaskItemRepository.UpdateItem(item).ConfigureAwait(false);
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
