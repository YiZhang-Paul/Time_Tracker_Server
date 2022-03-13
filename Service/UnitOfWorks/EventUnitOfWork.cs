using Core.DbContexts;
using Core.Interfaces.Repositories;
using Core.Interfaces.UnitOfWorks;
using System.Threading.Tasks;

namespace Service.UnitOfWorks
{
    public class EventUnitOfWork : IEventUnitOfWork
    {
        public IEventHistoryRepository EventHistory { get; }
        public IEventHistorySummaryRepository EventHistorySummary { get; }
        public IEventPromptRepository EventPrompt { get; }
        private TimeTrackerDbContext Context { get; }

        public EventUnitOfWork
        (
            TimeTrackerDbContext context,
            IEventHistoryRepository eventHistoryRepository,
            IEventHistorySummaryRepository eventHistorySummaryRepository,
            IEventPromptRepository eventPromptRepository
        )
        {
            Context = context;
            EventHistory = eventHistoryRepository;
            EventHistorySummary = eventHistorySummaryRepository;
            EventPrompt = eventPromptRepository;
        }

        public async Task<bool> Save()
        {
            return await Context.SaveChangesAsync().ConfigureAwait(false) > 0;
        }
    }
}
