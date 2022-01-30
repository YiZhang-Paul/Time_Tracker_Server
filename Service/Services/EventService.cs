using Core.Dtos;
using Core.Enums;
using Core.Interfaces.Repositories;
using Core.Interfaces.Services;
using Core.Models.Event;
using Service.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Service.Services
{
    public class EventService : IEventService
    {
        private IInterruptionItemRepository InterruptionItemRepository { get; }
        private ITaskItemRepository TaskItemRepository { get; }
        private IEventHistoryRepository EventHistoryRepository { get; }
        private IEventHistorySummaryRepository EventHistorySummaryRepository { get; }
        private IEventPromptRepository EventPromptRepository { get; }

        public EventService
        (
            IInterruptionItemRepository interruptionItemRepository,
            ITaskItemRepository taskItemRepository,
            IEventHistoryRepository eventHistoryRepository,
            IEventHistorySummaryRepository eventHistorySummaryRepository,
            IEventPromptRepository eventPromptRepository
        )
        {
            InterruptionItemRepository = interruptionItemRepository;
            TaskItemRepository = taskItemRepository;
            EventHistoryRepository = eventHistoryRepository;
            EventHistorySummaryRepository = eventHistorySummaryRepository;
            EventPromptRepository = eventPromptRepository;
        }

        public async Task<OngoingEventTimeSummaryDto> GetOngoingTimeSummary(DateTime start)
        {
            var lastPrompt = await EventPromptRepository.GetLastPrompt(PromptType.ScheduledBreak).ConfigureAwait(false);
            var startTime = start.ToUniversalTime();
            var promptTime = lastPrompt?.Timestamp ?? startTime;
            var endTime = DateTime.UtcNow;

            return new OngoingEventTimeSummaryDto
            {
                ConcludedSinceStart = await GetConcludedTimeSummary(startTime, endTime).ConfigureAwait(false),
                ConcludedSinceLastBreakPrompt = await GetConcludedTimeSummary(promptTime, endTime).ConfigureAwait(false),
                UnconcludedSinceStart = await GetUnconcludedTimeSummary(startTime).ConfigureAwait(false),
                UnconcludedSinceLastBreakPrompt = await GetUnconcludedTimeSummary(promptTime).ConfigureAwait(false)
            };
        }

        public async Task<List<EventHistorySummary>> GetEventHistorySummariesByDay(DateTime start)
        {
            var startTime = start.ToUniversalTime();
            var summaries = await EventHistorySummaryRepository.GetSummaries(startTime, startTime.AddDays(1)).ConfigureAwait(false);

            if (!summaries.Any() || summaries[0].Timestamp == startTime)
            {
                return summaries;
            }

            var previous = await EventHistorySummaryRepository.GetLastSummary(startTime).ConfigureAwait(false);

            if (previous != null)
            {
                previous.Timestamp = startTime;
                summaries.Insert(0, previous);
            }

            return summaries;
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

        public async Task<bool> StartBreakSession(int duration)
        {
            var minDuration = 1000 * 60 * 5;

            if (duration < minDuration)
            {
                throw new ArgumentException($"Duration cannot be less than {minDuration} milliseconds.");
            }

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

            var history = new EventHistory { ResourceId = -1, EventType = EventType.Break, TargetDuration = duration };

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
            var histories = await EventHistoryRepository.GetHistories(start, end).ConfigureAwait(false);

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

            return RecordTimeSummary(summary, previous?.EventType ?? EventType.Idling, start, histories[0].Timestamp);
        }

        private async Task<EventHistory> GetUnconcludedTimeSummary(DateTime start)
        {
            var history = await EventHistoryRepository.GetLastHistory(true).ConfigureAwait(false);
            history ??= new EventHistory { Id = -1, ResourceId = -1, EventType = EventType.Idling, Timestamp = start };
            history.Timestamp = (history.Timestamp > start ? history.Timestamp : start).SpecifyKindUtc();

            return history;
        }

        private static EventTimeSummary RecordTimeSummary(EventTimeSummary summary, EventType type, DateTime start, DateTime end)
        {
            var elapsed = (int)(end - start).TotalMilliseconds;

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
