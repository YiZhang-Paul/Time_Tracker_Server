using Core.DbContexts;
using Core.Interfaces.Repositories;
using Core.Interfaces.UnitOfWorks;
using System.Threading.Tasks;

namespace Service.UnitOfWorks
{
    public class WorkItemUnitOfWork : IWorkItemUnitOfWork
    {
        public IInterruptionItemRepository InterruptionItem { get; }
        public ITaskItemRepository TaskItem { get; }
        private TimeTrackerDbContext Context { get; }

        public WorkItemUnitOfWork
        (
            TimeTrackerDbContext context,
            IInterruptionItemRepository interruptionItemRepository,
            ITaskItemRepository taskItemRepository
        )
        {
            Context = context;
            InterruptionItem = interruptionItemRepository;
            TaskItem = taskItemRepository;
        }

        public async Task<bool> Save()
        {
            return await Context.SaveChangesAsync().ConfigureAwait(false) > 0;
        }
    }
}
