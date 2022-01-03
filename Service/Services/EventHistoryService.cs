using Core.Enums;
using Core.Interfaces.Repositories;
using Core.Interfaces.Services;
using Core.Models.EventHistory;
using System;
using System.Linq;
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

        public async Task<EventTimeDistribution> GetCurrentTimeDistribution()
        {
            var now = DateTime.UtcNow;
            var histories = await EventHistoryRepository.GetEventHistories(now.Date, now).ConfigureAwait(false);
            var lastHistory = await EventHistoryRepository.GetLastEventHistory().ConfigureAwait(false);
            var distribution = new EventTimeDistribution { Unconcluded = lastHistory };

            if (histories.Any())
            {
                for (var i = 0; i < histories.Count - 1; ++i)
                {
                    AddTimeDistribution(distribution, histories[i].EventType, histories[i].Timestamp, histories[i + 1].Timestamp);
                }

                var previous = await EventHistoryRepository.GetEventHistoryById(histories[0].Id - 1).ConfigureAwait(false);
                AddTimeDistribution(distribution, previous?.EventType ?? EventType.Idling, now.Date, histories[0].Timestamp);
            }
            else if (distribution.Unconcluded != null)
            {
                distribution.Unconcluded.Timestamp = now.Date;
            }

            return distribution;
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

        private static void AddTimeDistribution(EventTimeDistribution distribution, EventType type, DateTime start, DateTime end)
        {
            var elapsed = (int)(end - start).TotalMilliseconds;

            if (type == EventType.Idling)
            {
                distribution.Idling += elapsed;
            }
            else if (type == EventType.Interruption)
            {
                distribution.Interruption += elapsed;
            }
            else
            {
                distribution.Task += elapsed;
            }
        }
    }
}
