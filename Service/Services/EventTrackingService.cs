using Core.Dtos;
using Core.Enums;
using Core.Interfaces.Repositories;
using Core.Interfaces.UnitOfWorks;
using Core.Models.Event;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Service.Services
{
    public class EventTrackingService : IEventTrackingService
    {
        private IEventHistoryRepository EventHistoryRepository { get; }
        private IEventUnitOfWork EventUnitOfWork { get; }

        public EventTrackingService
        (
            IEventHistoryRepository eventHistoryRepository,
            IEventUnitOfWork eventUnitOfWork
        )
        {
            EventHistoryRepository = eventHistoryRepository;
            EventUnitOfWork = eventUnitOfWork;
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

            var last = await EventHistoryRepository.GetLastHistory().ConfigureAwait(false);
            var prompt = new EventPrompt { PromptType = PromptType.ScheduledBreak, ConfirmType = PromptConfirmType.Commenced };
            EventUnitOfWork.EventPrompt.CreatePrompt(prompt);

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
            EventUnitOfWork.EventPrompt.CreatePrompt(prompt);

            return await EventUnitOfWork.Save().ConfigureAwait(false);
        }

        public async Task<bool> UpdateTimeRange(EventTimeRangeDto range)
        {
            if (range.Start >= range.End)
            {
                throw new ArgumentException("Start time must come before end time.");
            }

            var histories = await GetAffectedHistories(range.Start, range.End).ConfigureAwait(false);
            var previous = histories.LastOrDefault(_ => _.Timestamp <= range.Start);
            var isSameEvent = previous?.EventType == range.EventType && previous?.ResourceId == range.Id;
            var isContained = histories.All(_ => _.Timestamp <= range.Start || _.Timestamp >= range.End);

            if (isSameEvent && isContained)
            {
                return await UpdateContainedTimeRange(histories, range).ConfigureAwait(false);
            }

            return await UpdateOverlapTimeRange(histories, range).ConfigureAwait(false);
        }

        private async Task<bool> UpdateContainedTimeRange(List<EventHistory> histories, EventTimeRangeDto range)
        {
            if (range.EventType == EventType.Idling)
            {
                return true;
            }

            if (!await UpdateContainedTimeRangeStart(histories, range).ConfigureAwait(false))
            {
                return false;
            }

            if (!await UpdateContainedTimeRangeEnd(histories, range.End).ConfigureAwait(false))
            {
                return false;
            }

            var start = histories.FirstOrDefault()?.Timestamp ?? range.Start;
            var end = histories.LastOrDefault()?.Timestamp ?? range.End;

            return await MergeTimeRanges(start, end).ConfigureAwait(false);
        }

        private async Task<bool> UpdateContainedTimeRangeStart(List<EventHistory> histories, EventTimeRangeDto range)
        {
            var previous = histories.LastOrDefault(_ => _.Timestamp <= range.Start);

            if (previous.Timestamp == range.Start)
            {
                return true;
            }

            var replaced = new EventHistory { ResourceId = -1, EventType = EventType.Idling, Timestamp = previous.Timestamp };

            if (!await EventHistoryRepository.DeleteHistory(previous).ConfigureAwait(false) || await EventHistoryRepository.CreateHistory(replaced).ConfigureAwait(false) == null)
            {
                return false;
            }

            var history = new EventHistory { ResourceId = range.Id, EventType = range.EventType, Timestamp = range.Start };

            return await EventHistoryRepository.CreateHistory(history).ConfigureAwait(false) != null;
        }

        private async Task<bool> UpdateContainedTimeRangeEnd(List<EventHistory> histories, DateTime end)
        {
            if (histories.Any(_ => _.Timestamp == end))
            {
                return true;
            }

            var history = new EventHistory { ResourceId = -1, EventType = EventType.Idling, Timestamp = end };

            return await EventHistoryRepository.CreateHistory(history).ConfigureAwait(false) != null;
        }

        private async Task<bool> UpdateOverlapTimeRange(List<EventHistory> histories, EventTimeRangeDto range)
        {
            if (!await UpdateOverlapTimeRangeStart(histories, range).ConfigureAwait(false))
            {
                return false;
            }

            if (!await UpdateOverlapTimeRangeEnd(histories, range.End).ConfigureAwait(false))
            {
                return false;
            }

            var overlaps = histories.Where(_ => _.Timestamp > range.Start && _.Timestamp < range.End).ToList();

            if (overlaps.Any() && !await EventHistoryRepository.DeleteHistories(overlaps).ConfigureAwait(false))
            {
                return false;
            }

            var start = histories.FirstOrDefault()?.Timestamp ?? range.Start;
            var end = histories.LastOrDefault()?.Timestamp ?? range.End;

            return await MergeTimeRanges(start, end).ConfigureAwait(false);
        }

        private async Task<bool> UpdateOverlapTimeRangeStart(List<EventHistory> histories, EventTimeRangeDto range)
        {
            var existing = histories.FirstOrDefault(_ => _.Timestamp == range.Start);

            if (existing != null && !await EventHistoryRepository.DeleteHistory(existing).ConfigureAwait(false))
            {
                return false;
            }

            var history = new EventHistory { ResourceId = range.Id, EventType = range.EventType, Timestamp = range.Start };

            return await EventHistoryRepository.CreateHistory(history).ConfigureAwait(false) != null;
        }

        private async Task<bool> UpdateOverlapTimeRangeEnd(List<EventHistory> histories, DateTime end)
        {
            if (histories.Any(_ => _.Timestamp == end))
            {
                return true;
            }

            var previous = histories.LastOrDefault(_ => _.Timestamp < end);
            var resourceId = previous?.ResourceId ?? -1;
            var eventType = previous?.EventType ?? EventType.Idling;
            var history = new EventHistory { ResourceId = resourceId, EventType = eventType, Timestamp = end };

            return await EventHistoryRepository.CreateHistory(history).ConfigureAwait(false) != null;
        }

        private async Task<bool> MergeTimeRanges(DateTime start, DateTime end)
        {
            var redundant = new List<EventHistory>();
            var histories = await GetAffectedHistories(start, end).ConfigureAwait(false);

            for (var i = 1; i < histories.Count; ++i)
            {
                var previous = histories[i - 1];
                var current = histories[i];

                if (previous.ResourceId == current.ResourceId && previous.EventType == current.EventType)
                {
                    redundant.Add(histories[i]);
                }
            }

            return !redundant.Any() || await EventHistoryRepository.DeleteHistories(redundant).ConfigureAwait(false);
        }

        private async Task<List<EventHistory>> GetAffectedHistories(DateTime start, DateTime end)
        {
            var previous = await EventHistoryRepository.GetLastHistory(start.AddTicks(-1000)).ConfigureAwait(false);
            var next = await EventHistoryRepository.GetNextHistory(end.AddTicks(1000)).ConfigureAwait(false);

            return await EventHistoryRepository.GetHistories(previous?.Timestamp ?? start, next?.Timestamp ?? end).ConfigureAwait(false);
        }
    }
}
