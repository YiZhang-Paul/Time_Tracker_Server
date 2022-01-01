using Core.Enums;
using Core.Interfaces.Repositories;
using Core.Interfaces.Services;
using Core.Models.EventHistory;
using Core.Models.Interruption;
using Core.Models.Task;
using System.Threading.Tasks;

namespace Service.Services
{
    public class EventHistoryService : IEventHistoryService
    {
        private IEventHistoryRepository EventHistoryRepository { get; }

        public EventHistoryService(IEventHistoryRepository eventHistoryRepository)
        {
            EventHistoryRepository = eventHistoryRepository;
        }

        public async Task<bool> StartIdlingSession()
        {
            var last = await EventHistoryRepository.GetLastEventHistory().ConfigureAwait(false);

            if (last?.EventType == EventType.Idling)
            {
                return false;
            }

            var history = new EventHistory { ResourceId = -1, EventType = EventType.Idling };

            return await EventHistoryRepository.CreateEventHistory(history).ConfigureAwait(false) != null;
        }

        public async Task<bool> StartInterruptionItem(InterruptionItem item)
        {
            var last = await EventHistoryRepository.GetLastEventHistory().ConfigureAwait(false);

            if (last?.EventType == EventType.Interruption && last?.ResourceId == item.Id)
            {
                return false;
            }

            var history = new EventHistory { ResourceId = item.Id, EventType = EventType.Interruption };

            return await EventHistoryRepository.CreateEventHistory(history).ConfigureAwait(false) != null;
        }

        public async Task<bool> StartTaskItem(TaskItem item)
        {
            var last = await EventHistoryRepository.GetLastEventHistory().ConfigureAwait(false);

            if (last?.EventType == EventType.Task && last?.ResourceId == item.Id)
            {
                return false;
            }

            var history = new EventHistory { ResourceId = item.Id, EventType = EventType.Task };

            return await EventHistoryRepository.CreateEventHistory(history).ConfigureAwait(false) != null;
        }
    }
}
