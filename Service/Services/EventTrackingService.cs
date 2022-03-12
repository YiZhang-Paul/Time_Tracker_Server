using Core.Dtos;
using Core.Enums;
using Core.Interfaces.Repositories;
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
        private IEventPromptRepository EventPromptRepository { get; }

        public EventTrackingService
        (
            IEventHistoryRepository eventHistoryRepository,
            IEventPromptRepository eventPromptRepository
        )
        {
            EventHistoryRepository = eventHistoryRepository;
            EventPromptRepository = eventPromptRepository;
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

        public async Task<bool> UpdateTimeRange(EventTimeRangeDto range)
        {
            if (range.Start >= range.End)
            {
                throw new ArgumentException("Start time must come before end time.");
            }

            var previous = await EventHistoryRepository.GetLastHistory(range.Start.AddTicks(-1000)).ConfigureAwait(false);
            var next = await EventHistoryRepository.GetNextHistory(range.End.AddTicks(1000)).ConfigureAwait(false);
            var start = previous?.Timestamp ?? range.Start;
            var end = next?.Timestamp ?? range.End;
            var histories = await EventHistoryRepository.GetHistories(start, end).ConfigureAwait(false);

            var previousHistory = histories.LastOrDefault(_ => _.Timestamp <= range.Start);

            if (previousHistory?.EventType == range.EventType && previousHistory.ResourceId == range.Id && histories.All(_ => _.Timestamp < range.Start || _.Timestamp >= range.End || _.EventType == range.EventType))
            {
                if (range.EventType == EventType.Idling)
                {
                    return true;
                }

                if (previousHistory.Timestamp != range.Start)
                {
                    var replacedPreviousHistory = new EventHistory { ResourceId = -1, EventType = EventType.Idling, Timestamp = previousHistory.Timestamp };
                    var history = new EventHistory { ResourceId = range.Id, EventType = range.EventType, Timestamp = range.Start };
                    await EventHistoryRepository.DeleteHistory(previousHistory).ConfigureAwait(false);
                    await EventHistoryRepository.CreateHistory(replacedPreviousHistory).ConfigureAwait(false);
                    await EventHistoryRepository.CreateHistory(history).ConfigureAwait(false);
                }

                if (histories.All(_ => _.Timestamp != range.End))
                {
                    var history = new EventHistory { ResourceId = -1, EventType = EventType.Idling, Timestamp = range.End };
                    await EventHistoryRepository.CreateHistory(history).ConfigureAwait(false);
                }
            }
            else
            {
                if (!await UpdateTimeRangeStart(histories, range).ConfigureAwait(false) || !await UpdateTimeRangeEnd(histories, range.End).ConfigureAwait(false))
                {
                    return false;
                }

                var overlaps = histories.Where(_ => _.Timestamp > range.Start && _.Timestamp < range.End).ToList();

                if (overlaps.Any() && !await EventHistoryRepository.DeleteHistories(overlaps).ConfigureAwait(false))
                {
                    return false;
                }
            }

            return await MergeTimeRange(start, end).ConfigureAwait(false);
        }

        private async Task<bool> UpdateTimeRangeStart(List<EventHistory> histories, EventTimeRangeDto range)
        {
            var existing = histories.FirstOrDefault(_ => _.Timestamp == range.Start);

            if (existing != null && !await EventHistoryRepository.DeleteHistory(existing).ConfigureAwait(false))
            {
                return false;
            }

            var history = new EventHistory { ResourceId = range.Id, EventType = range.EventType, Timestamp = range.Start };

            return await EventHistoryRepository.CreateHistory(history).ConfigureAwait(false) != null;
        }

        private async Task<bool> UpdateTimeRangeEnd(List<EventHistory> histories, DateTime end)
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

        private async Task<bool> MergeTimeRange(DateTime start, DateTime end)
        {
            var redundant = new List<EventHistory>();
            var previous = await EventHistoryRepository.GetLastHistory(start.AddTicks(-1000)).ConfigureAwait(false);
            var next = await EventHistoryRepository.GetNextHistory(end.AddTicks(1000)).ConfigureAwait(false);
            var startTime = previous?.Timestamp ?? start;
            var endTime = next?.Timestamp ?? end;
            var histories = await EventHistoryRepository.GetHistories(startTime, endTime).ConfigureAwait(false);

            for (var i = 1; i < histories.Count; ++i)
            {
                if (histories[i].ResourceId == histories[i - 1].ResourceId && histories[i].EventType == histories[i - 1].EventType)
                {
                    redundant.Add(histories[i]);
                }
            }

            return !redundant.Any() || await EventHistoryRepository.DeleteHistories(redundant).ConfigureAwait(false);
        }
    }
}
