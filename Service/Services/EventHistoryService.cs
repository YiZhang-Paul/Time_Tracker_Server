using Core.Enums;
using Core.Interfaces.Repositories;
using Core.Interfaces.Services;
using Core.Models.EventHistory;
using System.Threading.Tasks;

namespace Service.Services
{
    public class EventHistoryService : IEventHistoryService
    {
        private IInterruptionItemRepository InterruptionItemRepository { get; }
        private ITaskItemRepository TaskItemRepository { get; }
        private IEventHistoryRepository EventHistoryRepository { get; }

        public EventHistoryService
        (
            IInterruptionItemRepository interruptionItemRepository,
            ITaskItemRepository taskItemRepository,
            IEventHistoryRepository eventHistoryRepository
        )
        {
            InterruptionItemRepository = interruptionItemRepository;
            TaskItemRepository = taskItemRepository;
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

        public async Task<bool> StartInterruptionItem(long id)
        {
            var item = await InterruptionItemRepository.GetInterruptionItemById(id).ConfigureAwait(false);

            if (item == null)
            {
                return false;
            }

            var last = await EventHistoryRepository.GetLastEventHistory().ConfigureAwait(false);

            if (last?.EventType == EventType.Interruption && last?.ResourceId == id)
            {
                return false;
            }

            var history = new EventHistory { ResourceId = id, EventType = EventType.Interruption };

            return await EventHistoryRepository.CreateEventHistory(history).ConfigureAwait(false) != null;
        }

        public async Task<bool> StartTaskItem(long id)
        {
            var item = await TaskItemRepository.GetTaskItemById(id).ConfigureAwait(false);

            if (item == null)
            {
                return false;
            }

            var last = await EventHistoryRepository.GetLastEventHistory().ConfigureAwait(false);

            if (last?.EventType == EventType.Task && last?.ResourceId == id)
            {
                return false;
            }

            var history = new EventHistory { ResourceId = id, EventType = EventType.Task };

            return await EventHistoryRepository.CreateEventHistory(history).ConfigureAwait(false) != null;
        }
    }
}
