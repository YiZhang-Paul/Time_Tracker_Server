using Core.Enums;
using Core.Interfaces.Repositories;
using Core.Interfaces.Services;
using Core.Models.Event;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Service.Services
{
    public class EventService : IEventService
    {
        private IInterruptionItemRepository InterruptionItemRepository { get; }
        private ITaskItemRepository TaskItemRepository { get; }
        private IEventHistoryRepository EventHistoryRepository { get; }
        private IEventPromptRepository EventPromptRepository { get; }

        public EventService
        (
            IInterruptionItemRepository interruptionItemRepository,
            ITaskItemRepository taskItemRepository,
            IEventHistoryRepository eventHistoryRepository,
            IEventPromptRepository eventPromptRepository
        )
        {
            InterruptionItemRepository = interruptionItemRepository;
            TaskItemRepository = taskItemRepository;
            EventHistoryRepository = eventHistoryRepository;
            EventPromptRepository = eventPromptRepository;
        }

        public async Task<OngoingEventTimeSummary> GetOngoingTimeSummary(DateTime start)
        {
            var lastPrompt = await EventPromptRepository.GetLastPrompt(PromptType.ScheduledBreak).ConfigureAwait(false);
            var startTime = start.ToUniversalTime();
            var promptTime = lastPrompt?.Timestamp ?? startTime;
            var endTime = DateTime.UtcNow;

            return new OngoingEventTimeSummary
            {
                ConcludedSinceStart = await GetConcludedTimeSummary(startTime, endTime).ConfigureAwait(false),
                ConcludedSinceLastBreakPrompt = await GetConcludedTimeSummary(promptTime, endTime).ConfigureAwait(false),
                UnconcludedSinceStart = await GetUnconcludedTimeSummary(startTime).ConfigureAwait(false),
                UnconcludedSinceLastBreakPrompt = await GetUnconcludedTimeSummary(promptTime).ConfigureAwait(false)
            };
        }

        public async Task<bool> StartIdlingSession()
        {
            var last = await EventHistoryRepository.GetLastHistory().ConfigureAwait(false);

            if (last?.EventType == EventType.Idling)
            {
                return false;
            }

            var history = new EventHistory { ResourceId = -1, EventType = EventType.Idling };

            return await EventHistoryRepository.CreateHistory(history).ConfigureAwait(false) != null;
        }

        public async Task<bool> StartInterruptionItem(long id)
        {
            var item = await InterruptionItemRepository.GetItemById(id).ConfigureAwait(false);

            if (item == null)
            {
                return false;
            }

            var last = await EventHistoryRepository.GetLastHistory().ConfigureAwait(false);

            if (last != null && last.EventType == EventType.Interruption && last.ResourceId == id)
            {
                return false;
            }

            var history = new EventHistory { ResourceId = id, EventType = EventType.Interruption };

            return await EventHistoryRepository.CreateHistory(history).ConfigureAwait(false) != null;
        }

        public async Task<bool> StartTaskItem(long id)
        {
            var item = await TaskItemRepository.GetItemById(id).ConfigureAwait(false);

            if (item == null)
            {
                return false;
            }

            var last = await EventHistoryRepository.GetLastHistory().ConfigureAwait(false);

            if (last != null && last.EventType == EventType.Task && last.ResourceId == id)
            {
                return false;
            }

            var history = new EventHistory { ResourceId = id, EventType = EventType.Task };

            return await EventHistoryRepository.CreateHistory(history).ConfigureAwait(false) != null;
        }

        public async Task<bool> StartBreakSession()
        {
            var prompt = new EventPrompt { PromptType = PromptType.ScheduledBreak, ConfirmType = PromptConfirmType.Commenced };

            if (await EventPromptRepository.CreatePrompt(prompt).ConfigureAwait(false) == null)
            {
                return false;
            }

            var last = await EventHistoryRepository.GetLastHistory().ConfigureAwait(false);

            if (last?.EventType == EventType.Break)
            {
                return false;
            }

            var history = new EventHistory { ResourceId = -1, EventType = EventType.Break };

            return await EventHistoryRepository.CreateHistory(history).ConfigureAwait(false) != null;
        }

        public async Task<bool> SkipBreakSession()
        {
            var prompt = new EventPrompt { PromptType = PromptType.ScheduledBreak, ConfirmType = PromptConfirmType.Skipped };

            return await EventPromptRepository.CreatePrompt(prompt).ConfigureAwait(false) != null;
        }

        private async Task<EventTimeSummary> GetConcludedTimeSummary(DateTime start, DateTime end)
        {
            var summary = new EventTimeSummary();
            var startTime = start.ToUniversalTime();
            var endTime = end.ToUniversalTime();
            var histories = await EventHistoryRepository.GetHistories(startTime, endTime).ConfigureAwait(false);

            if (!histories.Any())
            {
                return summary;
            }
            // record time between histories
            for (var i = 0; i < histories.Count - 1; ++i)
            {
                summary = RecordTimeSummary(summary, histories[i].EventType, histories[i].Timestamp, histories[i + 1].Timestamp);
            }
            // record time between start time and first history
            var previous = await EventHistoryRepository.GetHistoryById(histories[0].Id - 1).ConfigureAwait(false);

            return RecordTimeSummary(summary, previous?.EventType ?? EventType.Idling, startTime, histories[0].Timestamp);
        }

        private async Task<EventHistory> GetUnconcludedTimeSummary(DateTime start)
        {
            var startTime = start.ToUniversalTime();
            var history = await EventHistoryRepository.GetLastHistory().ConfigureAwait(false);
            history ??= new EventHistory { Id = -1, ResourceId = -1, EventType = EventType.Idling, Timestamp = startTime };
            history.Timestamp = history.Timestamp.ToUniversalTime() > startTime ? history.Timestamp.ToUniversalTime() : startTime;

            return history;
        }

        private static EventTimeSummary RecordTimeSummary(EventTimeSummary summary, EventType type, DateTime start, DateTime end)
        {
            var elapsed = (int)(end.ToUniversalTime() - start.ToUniversalTime()).TotalMilliseconds;

            if (type == EventType.Interruption || type == EventType.Task)
            {
                summary.Working += elapsed;
            }
            else if (type == EventType.Idling || type == EventType.Break)
            {
                summary.NotWorking += elapsed;
            }

            return summary;
        }
    }
}
