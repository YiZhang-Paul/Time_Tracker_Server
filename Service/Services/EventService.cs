using Core.Dtos;
using Core.Enums;
using Core.Interfaces.Repositories;
using Core.Interfaces.Services;
using Core.Models.Event;
using Core.Models.Generic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Service.Services
{
    public class EventService : IEventService
    {
        private IEventHistoryRepository EventHistoryRepository { get; }
        private IEventHistorySummaryRepository EventHistorySummaryRepository { get; }
        private IEventPromptRepository EventPromptRepository { get; }

        public EventService
        (
            IEventHistoryRepository eventHistoryRepository,
            IEventHistorySummaryRepository eventHistorySummaryRepository,
            IEventPromptRepository eventPromptRepository
        )
        {
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

        public async Task<EventSummariesDto> GetEventSummariesByDay(DateTime start)
        {
            var startTime = start.ToUniversalTime();
            var endTime = startTime.AddDays(1) < DateTime.UtcNow ? startTime.AddDays(1) : DateTime.UtcNow;

            if (startTime > DateTime.UtcNow)
            {
                return new EventSummariesDto();
            }

            var histories = await GetEventHistorySummariesByDay(startTime).ConfigureAwait(false);

            if (!histories.Any())
            {
                return new EventSummariesDto();
            }

            var summaries = new EventSummariesDto { Timeline = histories.Select(EventTimelineDto.Convert).ToList() };
            // record time between timeline events
            for (var i = 0; i < summaries.Timeline.Count - 1; ++i)
            {
                summaries = RecordEventDuration(summaries, summaries.Timeline[i], summaries.Timeline[i + 1].StartTime);
            }
            // record time between last timeline event and end time
            summaries = RecordEventDuration(summaries, summaries.Timeline.Last(), endTime);
            summaries.Duration = summaries.Duration.OrderByDescending(_ => _.Duration).ToList();

            return summaries;
        }

        public async Task<List<string>> GetTimesheetsByDay(DateTime start)
        {
            var summaries = await GetEventSummariesByDay(start).ConfigureAwait(false);
            var durations = summaries.Duration.Where(_ => _.EventType != EventType.Idling && _.EventType != EventType.Break);

            return durations.Select(_ =>
            {
                var time = TimeSpan.FromMilliseconds(_.Duration);
                var total = time.TotalMinutes < 1 ? "<1m" : $"{Math.Round(time.TotalMinutes)}m";

                return $"{time.Hours}h {time.Minutes}m ({total}) - {_.Name}";
            }).ToList();
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

            var t1 = range.Start == range.Start.AddTicks(-1000);
            var t2 = range.End == range.End.AddTicks(1000);
            var previous = await EventHistoryRepository.GetLastHistory(range.Start.AddTicks(-1000)).ConfigureAwait(false);
            var next = await EventHistoryRepository.GetNextHistory(range.End.AddTicks(1000)).ConfigureAwait(false);
            var events = await EventHistoryRepository.GetHistories(previous?.Timestamp ?? range.Start, next?.Timestamp ?? range.End).ConfigureAwait(false);

            if (events.All(_ => _.Timestamp != range.End))
            {
                var before = events.LastOrDefault(_ => _.Timestamp < range.End);
                var end = new EventHistory { ResourceId = before?.ResourceId ?? -1, EventType = before?.EventType ?? EventType.Idling, Timestamp = range.End };
                await EventHistoryRepository.CreateHistory(end).ConfigureAwait(false);
            }

            var matchingStart = events.FirstOrDefault(_ => _.Timestamp == range.Start);

            if (matchingStart != null)
            {
                await EventHistoryRepository.DeleteHistory(matchingStart).ConfigureAwait(false);
            }

            var start = new EventHistory { ResourceId = range.Id, EventType = range.EventType, Timestamp = range.Start };
            var overlaps = events.Where(_ => _.Timestamp > range.Start && _.Timestamp < range.End).ToList();
            await EventHistoryRepository.CreateHistory(start).ConfigureAwait(false);
            await EventHistoryRepository.DeleteHistories(overlaps).ConfigureAwait(false);
            var mergeable = new List<EventHistory>();
            events = await EventHistoryRepository.GetHistories(previous?.Timestamp ?? range.Start, next?.Timestamp ?? range.End).ConfigureAwait(false);

            for (var i = 1; i < events.Count; ++i)
            {
                if (events[i].ResourceId == events[i - 1].ResourceId && events[i].EventType == events[i - 1].EventType)
                {
                    mergeable.Add(events[i]);
                }
            }

            await EventHistoryRepository.DeleteHistories(mergeable).ConfigureAwait(false);

            return true;
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
            var history = await EventHistoryRepository.GetLastHistory(null, true).ConfigureAwait(false);
            history ??= new EventHistory { Id = -1, ResourceId = -1, EventType = EventType.Idling, Timestamp = start };
            history.Timestamp = history.Timestamp > start ? history.Timestamp : start;

            return history;
        }

        private async Task<List<EventHistorySummary>> GetEventHistorySummariesByDay(DateTime start)
        {
            var summaries = await EventHistorySummaryRepository.GetSummaries(start, start.AddDays(1)).ConfigureAwait(false);

            if (!summaries.Any() || summaries[0].Timestamp != start)
            {
                var previous = await EventHistorySummaryRepository.GetLastSummary(start).ConfigureAwait(false);
                previous.Timestamp = start;
                summaries.Insert(0, previous);
            }

            return summaries;
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

        private static EventSummariesDto RecordEventDuration(EventSummariesDto summaries, EventTimelineDto timeline, DateTime end)
        {
            var existing = summaries.Duration.Find(_ => _.EventType == timeline.EventType && _.Id == timeline.Id);

            if (existing == null)
            {
                existing = EventDurationDto.Convert(timeline);
                summaries.Duration.Add(existing);
            }

            existing.Duration += (int)(end - timeline.StartTime).TotalMilliseconds;
            existing.Periods.Add(new TimePeriod { Start = timeline.StartTime, End = end });

            return summaries;
        }
    }
}
