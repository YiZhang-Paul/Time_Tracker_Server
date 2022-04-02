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

        public async Task<bool> StartIdlingSession(long userId)
        {
            var last = await EventUnitOfWork.EventHistory.GetLastHistory(userId).ConfigureAwait(false);

            if (last?.EventType == EventType.Idling)
            {
                return false;
            }

            var history = new EventHistory { UserId = userId, ResourceId = -1, EventType = EventType.Idling };
            EventUnitOfWork.EventHistory.CreateHistory(history);

            return await EventUnitOfWork.Save().ConfigureAwait(false);
        }

        public async Task<bool> StartInterruptionItem(long userId, long id)
        {
            var last = await EventUnitOfWork.EventHistory.GetLastHistory(userId).ConfigureAwait(false);

            if (last != null && last.EventType == EventType.Interruption && last.ResourceId == id)
            {
                return false;
            }

            var history = new EventHistory { UserId = userId, ResourceId = id, EventType = EventType.Interruption };
            EventUnitOfWork.EventHistory.CreateHistory(history);

            return await EventUnitOfWork.Save().ConfigureAwait(false);
        }

        public async Task<bool> StartTaskItem(long userId, long id)
        {
            var last = await EventUnitOfWork.EventHistory.GetLastHistory(userId).ConfigureAwait(false);

            if (last != null && last.EventType == EventType.Task && last.ResourceId == id)
            {
                return false;
            }

            var history = new EventHistory { UserId = userId, ResourceId = id, EventType = EventType.Task };
            EventUnitOfWork.EventHistory.CreateHistory(history);

            return await EventUnitOfWork.Save().ConfigureAwait(false);
        }

        public async Task<bool> StartBreakSession(long userId, int duration)
        {
            var minDuration = 1000 * 60 * 5;

            if (duration < minDuration)
            {
                throw new ArgumentException($"Duration cannot be less than {minDuration} milliseconds.");
            }

            var prompt = new EventPrompt { UserId = userId, PromptType = PromptType.ScheduledBreak, ConfirmType = PromptConfirmType.Commenced };
            EventUnitOfWork.EventPrompt.CreatePrompt(prompt);

            if (!await EventUnitOfWork.Save().ConfigureAwait(false))
            {
                return false;
            }

            var last = await EventUnitOfWork.EventHistory.GetLastHistory(userId).ConfigureAwait(false);

            if (last?.EventType == EventType.Break)
            {
                return false;
            }

            var history = new EventHistory { UserId = userId, ResourceId = -1, EventType = EventType.Break, TargetDuration = duration };
            EventUnitOfWork.EventHistory.CreateHistory(history);

            return await EventUnitOfWork.Save().ConfigureAwait(false);
        }

        public async Task<bool> SkipBreakSession(long userId)
        {
            var prompt = new EventPrompt { UserId = userId, PromptType = PromptType.ScheduledBreak, ConfirmType = PromptConfirmType.Skipped };
            EventUnitOfWork.EventPrompt.CreatePrompt(prompt);

            return await EventUnitOfWork.Save().ConfigureAwait(false);
        }

        public async Task<bool> UpdateTimeRange(long userId, EventTimeRangeDto range)
        {
            if (range.Start >= range.End)
            {
                throw new ArgumentException("Start time must come before end time.");
            }

            var histories = await GetAffectedHistories(userId, range.Start, range.End).ConfigureAwait(false);
            var previous = histories.LastOrDefault(_ => _.Timestamp <= range.Start);
            var isSameEvent = previous?.EventType == range.EventType && previous?.ResourceId == range.Id;
            var isContained = isSameEvent && histories.All(_ => _.Timestamp <= range.Start || _.Timestamp >= range.End);

            if (isContained && !await UpdateContainedTimeRange(userId, histories, range).ConfigureAwait(false))
            {
                return false;
            }

            if (!isContained && !await UpdateOverlapTimeRange(userId, histories, range).ConfigureAwait(false))
            {
                return false;
            }

            var start = histories.FirstOrDefault()?.Timestamp ?? range.Start;
            var end = histories.LastOrDefault()?.Timestamp ?? range.End;

            return await MergeTimeRanges(userId, start, end).ConfigureAwait(false);
        }

        private async Task<bool> UpdateContainedTimeRange(long userId, List<EventHistory> histories, EventTimeRangeDto range)
        {
            if (range.EventType == EventType.Idling)
            {
                return true;
            }

            SetContainedTimeRangeStart(userId, histories, range);
            SetContainedTimeRangeEnd(userId, histories, range.End);

            return await EventUnitOfWork.Save().ConfigureAwait(false);
        }

        private void SetContainedTimeRangeStart(long userId, List<EventHistory> histories, EventTimeRangeDto range)
        {
            var previous = histories.LastOrDefault(_ => _.Timestamp <= range.Start);

            if (previous.Timestamp == range.Start)
            {
                return;
            }

            var history = new EventHistory
            {
                UserId = userId,
                ResourceId = range.Id,
                EventType = range.EventType,
                Timestamp = range.Start
            };

            EventUnitOfWork.EventHistory.CreateHistory(history);
            // mark original range start as idling
            var replaced = new EventHistory
            {
                UserId = userId,
                ResourceId = -1,
                EventType = EventType.Idling,
                Timestamp = previous.Timestamp
            };

            EventUnitOfWork.EventHistory.CreateHistory(replaced);
            EventUnitOfWork.EventHistory.DeleteHistory(previous);
        }

        private void SetContainedTimeRangeEnd(long userId, List<EventHistory> histories, DateTime end)
        {
            if (histories.All(_ => _.Timestamp != end))
            {
                var history = new EventHistory
                {
                    UserId = userId,
                    ResourceId = -1,
                    EventType = EventType.Idling,
                    Timestamp = end
                };

                EventUnitOfWork.EventHistory.CreateHistory(history);
            }
        }

        private async Task<bool> UpdateOverlapTimeRange(long userId, List<EventHistory> histories, EventTimeRangeDto range)
        {
            SetOverlapTimeRangeStart(userId, histories, range);
            SetOverlapTimeRangeEnd(userId, histories, range.End);
            var overlaps = histories.Where(_ => _.Timestamp > range.Start && _.Timestamp < range.End).ToList();

            if (overlaps.Any())
            {
                EventUnitOfWork.EventHistory.DeleteHistories(overlaps);
            }

            return await EventUnitOfWork.Save().ConfigureAwait(false);
        }

        private void SetOverlapTimeRangeStart(long userId, List<EventHistory> histories, EventTimeRangeDto range)
        {
            var existing = histories.FirstOrDefault(_ => _.Timestamp == range.Start);

            if (existing != null)
            {
                EventUnitOfWork.EventHistory.DeleteHistory(existing);
            }

            var history = new EventHistory
            {
                UserId = userId,
                ResourceId = range.Id,
                EventType = range.EventType,
                Timestamp = range.Start
            };

            EventUnitOfWork.EventHistory.CreateHistory(history);
        }

        private void SetOverlapTimeRangeEnd(long userId, List<EventHistory> histories, DateTime end)
        {
            if (histories.Any(_ => _.Timestamp == end))
            {
                return;
            }

            var previous = histories.LastOrDefault(_ => _.Timestamp < end);

            var history = new EventHistory
            {
                UserId = userId,
                ResourceId = previous?.ResourceId ?? -1,
                EventType = previous?.EventType ?? EventType.Idling,
                Timestamp = end
            };

            EventUnitOfWork.EventHistory.CreateHistory(history);
        }

        private async Task<bool> MergeTimeRanges(long userId, DateTime start, DateTime end)
        {
            var redundant = new List<EventHistory>();
            var histories = await GetAffectedHistories(userId, start, end).ConfigureAwait(false);

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

        private async Task<List<EventHistory>> GetAffectedHistories(long userId, DateTime start, DateTime end)
        {
            var previous = await EventUnitOfWork.EventHistory.GetLastHistory(userId, start.AddTicks(-1000)).ConfigureAwait(false);
            var next = await EventUnitOfWork.EventHistory.GetNextHistory(userId, end.AddTicks(1000)).ConfigureAwait(false);
            var startTime = previous?.Timestamp ?? start;
            var endTime = next?.Timestamp ?? end;

            return await EventUnitOfWork.EventHistory.GetHistories(userId, startTime, endTime).ConfigureAwait(false);
        }
    }
}
