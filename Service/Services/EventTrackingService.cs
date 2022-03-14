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
        private IEventUnitOfWork EventUnitOfWork { get; }

        public EventTrackingService(IEventUnitOfWork eventUnitOfWork)
        {
            EventUnitOfWork = eventUnitOfWork;
        }

        public async Task<bool> StartIdlingSession()
        {
            var last = await EventUnitOfWork.EventHistory.GetLastHistory().ConfigureAwait(false);

            if (last?.EventType == EventType.Idling)
            {
                return false;
            }

            var history = new EventHistory { ResourceId = -1, EventType = EventType.Idling };
            EventUnitOfWork.EventHistory.CreateHistory(history);

            return await EventUnitOfWork.Save().ConfigureAwait(false);
        }

        public async Task<bool> StartInterruptionItem(long id)
        {
            var last = await EventUnitOfWork.EventHistory.GetLastHistory().ConfigureAwait(false);

            if (last != null && last.EventType == EventType.Interruption && last.ResourceId == id)
            {
                return false;
            }

            var history = new EventHistory { ResourceId = id, EventType = EventType.Interruption };
            EventUnitOfWork.EventHistory.CreateHistory(history);

            return await EventUnitOfWork.Save().ConfigureAwait(false);
        }

        public async Task<bool> StartTaskItem(long id)
        {
            var last = await EventUnitOfWork.EventHistory.GetLastHistory().ConfigureAwait(false);

            if (last != null && last.EventType == EventType.Task && last.ResourceId == id)
            {
                return false;
            }

            var history = new EventHistory { ResourceId = id, EventType = EventType.Task };
            EventUnitOfWork.EventHistory.CreateHistory(history);

            return await EventUnitOfWork.Save().ConfigureAwait(false);
        }

        public async Task<bool> StartBreakSession(int duration)
        {
            var minDuration = 1000 * 60 * 5;

            if (duration < minDuration)
            {
                throw new ArgumentException($"Duration cannot be less than {minDuration} milliseconds.");
            }

            var prompt = new EventPrompt { PromptType = PromptType.ScheduledBreak, ConfirmType = PromptConfirmType.Commenced };
            EventUnitOfWork.EventPrompt.CreatePrompt(prompt);

            if (!await EventUnitOfWork.Save().ConfigureAwait(false))
            {
                return false;
            }

            var last = await EventUnitOfWork.EventHistory.GetLastHistory().ConfigureAwait(false);

            if (last?.EventType == EventType.Break)
            {
                return false;
            }

            var history = new EventHistory { ResourceId = -1, EventType = EventType.Break, TargetDuration = duration };
            EventUnitOfWork.EventHistory.CreateHistory(history);

            return await EventUnitOfWork.Save().ConfigureAwait(false);
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
            var isContained = isSameEvent && histories.All(_ => _.Timestamp <= range.Start || _.Timestamp >= range.End);

            if (isContained && !await UpdateContainedTimeRange(histories, range).ConfigureAwait(false))
            {
                return false;
            }

            if (!isContained && !await UpdateOverlapTimeRange(histories, range).ConfigureAwait(false))
            {
                return false;
            }

            var start = histories.FirstOrDefault()?.Timestamp ?? range.Start;
            var end = histories.LastOrDefault()?.Timestamp ?? range.End;

            return await MergeTimeRanges(start, end).ConfigureAwait(false);
        }

        private async Task<bool> UpdateContainedTimeRange(List<EventHistory> histories, EventTimeRangeDto range)
        {
            if (range.EventType == EventType.Idling)
            {
                return true;
            }

            SetContainedTimeRangeStart(histories, range);
            SetContainedTimeRangeEnd(histories, range.End);

            return await EventUnitOfWork.Save().ConfigureAwait(false);
        }

        private void SetContainedTimeRangeStart(List<EventHistory> histories, EventTimeRangeDto range)
        {
            var previous = histories.LastOrDefault(_ => _.Timestamp <= range.Start);

            if (previous.Timestamp == range.Start)
            {
                return;
            }

            var history = new EventHistory { ResourceId = range.Id, EventType = range.EventType, Timestamp = range.Start };
            EventUnitOfWork.EventHistory.CreateHistory(history);
            // mark original range start as idling
            var replaced = new EventHistory { ResourceId = -1, EventType = EventType.Idling, Timestamp = previous.Timestamp };
            EventUnitOfWork.EventHistory.DeleteHistory(previous);
            EventUnitOfWork.EventHistory.CreateHistory(replaced);
        }

        private void SetContainedTimeRangeEnd(List<EventHistory> histories, DateTime end)
        {
            if (histories.All(_ => _.Timestamp != end))
            {
                var history = new EventHistory { ResourceId = -1, EventType = EventType.Idling, Timestamp = end };
                EventUnitOfWork.EventHistory.CreateHistory(history);
            }
        }

        private async Task<bool> UpdateOverlapTimeRange(List<EventHistory> histories, EventTimeRangeDto range)
        {
            SetOverlapTimeRangeStart(histories, range);
            SetOverlapTimeRangeEnd(histories, range.End);
            var overlaps = histories.Where(_ => _.Timestamp > range.Start && _.Timestamp < range.End).ToList();

            if (overlaps.Any())
            {
                EventUnitOfWork.EventHistory.DeleteHistories(overlaps);
            }

            return await EventUnitOfWork.Save().ConfigureAwait(false);
        }

        private void SetOverlapTimeRangeStart(List<EventHistory> histories, EventTimeRangeDto range)
        {
            var existing = histories.FirstOrDefault(_ => _.Timestamp == range.Start);

            if (existing != null)
            {
                EventUnitOfWork.EventHistory.DeleteHistory(existing);
            }

            var history = new EventHistory { ResourceId = range.Id, EventType = range.EventType, Timestamp = range.Start };
            EventUnitOfWork.EventHistory.CreateHistory(history);
        }

        private void SetOverlapTimeRangeEnd(List<EventHistory> histories, DateTime end)
        {
            if (histories.Any(_ => _.Timestamp == end))
            {
                return;
            }

            var previous = histories.LastOrDefault(_ => _.Timestamp < end);

            var history = new EventHistory
            {
                ResourceId = previous?.ResourceId ?? -1,
                EventType = previous?.EventType ?? EventType.Idling,
                Timestamp = end
            };

            EventUnitOfWork.EventHistory.CreateHistory(history);
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

            if (!redundant.Any())
            {
                return true;
            }

            EventUnitOfWork.EventHistory.DeleteHistories(redundant);

            return await EventUnitOfWork.Save().ConfigureAwait(false);
        }

        private async Task<List<EventHistory>> GetAffectedHistories(DateTime start, DateTime end)
        {
            var previous = await EventUnitOfWork.EventHistory.GetLastHistory(start.AddTicks(-1000)).ConfigureAwait(false);
            var next = await EventUnitOfWork.EventHistory.GetNextHistory(end.AddTicks(1000)).ConfigureAwait(false);

            return await EventUnitOfWork.EventHistory.GetHistories(previous?.Timestamp ?? start, next?.Timestamp ?? end).ConfigureAwait(false);
        }
    }
}
