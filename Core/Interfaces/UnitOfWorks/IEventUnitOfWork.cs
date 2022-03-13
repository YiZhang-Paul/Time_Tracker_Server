using Core.Interfaces.Repositories;
using System.Threading.Tasks;

namespace Core.Interfaces.UnitOfWorks
{
    public interface IEventUnitOfWork
    {
        IEventHistoryRepository EventHistory { get; }
        IEventHistorySummaryRepository EventHistorySummary { get; }
        IEventPromptRepository EventPrompt { get; }
        Task<bool> Save();
    }
}
