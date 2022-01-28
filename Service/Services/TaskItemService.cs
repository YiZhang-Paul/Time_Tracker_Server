using Core.Enums;
using Core.Interfaces.Repositories;
using Core.Interfaces.Services;
using Core.Models.Task;
using System;
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

        public async Task<TaskItem> UpdateItem(TaskItem item, ResolveAction action = ResolveAction.None)
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

            return await TaskItemRepository.UpdateItem(item).ConfigureAwait(false);
        }
    }
}
