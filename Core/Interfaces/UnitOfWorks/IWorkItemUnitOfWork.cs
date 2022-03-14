using Core.Interfaces.Repositories;
using System.Threading.Tasks;

namespace Core.Interfaces.UnitOfWorks
{
    public interface IWorkItemUnitOfWork
    {
        IInterruptionItemRepository InterruptionItem { get; }
        ITaskItemRepository TaskItem { get; }
        Task<bool> Save();
    }
}
