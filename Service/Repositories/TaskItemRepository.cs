using Core.DbContexts;
using Core.Dtos;
using Core.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Service.Repositories
{
    public class TaskItemRepository : ITaskItemRepository
    {
        private TimeTrackerDbContext Context { get; }

        public TaskItemRepository(TimeTrackerDbContext context)
        {
            Context = context;
        }

        public async Task<List<TaskItemDto>> GetTaskItemSummaries()
        {
            var items = Context.TaskItem.Select(_ => new TaskItemDto { Id = _.Id, Name = _.Name });

            return await items.ToListAsync().ConfigureAwait(false);
        }
    }
}
