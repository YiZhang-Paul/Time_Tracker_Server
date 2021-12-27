using Core.DbContexts;
using Core.Interfaces.Repositories;
using Core.Models.Task;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
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

        public async Task<List<TaskItem>> GetVisibleTaskItems()
        {
            return await Context.TaskItem.ToListAsync().ConfigureAwait(false);
        }
    }
}
