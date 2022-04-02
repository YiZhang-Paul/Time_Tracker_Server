using Core.Dtos;
using Core.Enums;
using Core.Interfaces.Services;
using Core.Interfaces.UnitOfWorks;
using Core.Models.Event;
using Core.Models.Generic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Service.Services
{
    public class EventSummaryService : IEventSummaryService
    {
        private IEventUnitOfWork EventUnitOfWork { get; }

        public EventSummaryService(IEventUnitOfWork eventUnitOfWork)
        {
            EventUnitOfWork = eventUnitOfWork;
        }

        public async Task<OngoingEventTimeSummaryDto> GetOngoingTimeSummary(long userId, DateTime start)
        {
            var lastPrompt = await EventUnitOfWork.EventPrompt.GetLastPrompt(userId, PromptType.ScheduledBreak).ConfigureAwait(false);
            var startTime = start.ToUniversalTime();
            var promptTime = lastPrompt?.Timestamp ?? startTime;
            var endTime = DateTime.UtcNow;

            return new OngoingEventTimeSummaryDto
            {
                ConcludedSinceStart = await GetConcludedTimeSummary(userId, startTime, endTime).ConfigureAwait(false),
                ConcludedSinceLastBreakPrompt = await GetConcludedTimeSummary(userId, promptTime, endTime).ConfigureAwait(false),
                UnconcludedSinceStart = await GetUnconcludedTimeSummary(userId, startTime).ConfigureAwait(false),
                UnconcludedSinceLastBreakPrompt = await GetUnconcludedTimeSummary(userId, promptTime).ConfigureAwait(false)
            };
        }

        public async Task<EventSummariesDto> GetEventSummariesByDay(long userId, DateTime start)
        {
            var startTime = start.ToUniversalTime();
            var endOfDay = startTime.AddDays(1).AddTicks(-1000);
            var endTime = endOfDay < DateTime.UtcNow ? endOfDay : DateTime.UtcNow;

            if (startTime > DateTime.UtcNow)
            {
                return new EventSummariesDto();
            }

            var histories = await GetEventHistorySummaries(userId, startTime, endTime).ConfigureAwait(false);

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

        public async Task<List<string>> GetTimesheetsByDay(long userId, DateTime start)
        {
            var summaries = await GetEventSummariesByDay(userId, start).ConfigureAwait(false);
            var durations = summaries.Duration.Where(_ => _.EventType != EventType.Idling && _.EventType != EventType.Break);

            return durations.Select(_ =>
            {
                var time = TimeSpan.FromMilliseconds(_.Duration);
                var total = time.TotalMinutes < 1 ? "<1m" : $"{Math.Round(time.TotalMinutes)}m";

                return $"{time.Hours}h {time.Minutes}m ({total}) - {_.Name}";
            }).ToList();
        }

        private async Task<EventTimeSummary> GetConcludedTimeSummary(long userId, DateTime start, DateTime end)
        {
            var summary = new EventTimeSummary();
            var histories = await EventUnitOfWork.EventHistory.GetHistories(userId, start, end).ConfigureAwait(false);

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
            var previous = await EventUnitOfWork.EventHistory.GetLastHistory(userId, histories[0].Timestamp.AddTicks(-1000)).ConfigureAwait(false);

            return RecordTimeSummary(summary, previous?.EventType ?? EventType.Idling, start, histories[0].Timestamp);
        }

        private async Task<EventHistory> GetUnconcludedTimeSummary(long userId, DateTime start)
        {
            var history = await EventUnitOfWork.EventHistory.GetLastHistory(userId, null, true).ConfigureAwait(false);
            history ??= new EventHistory { UserId = userId, Id = -1, ResourceId = -1, EventType = EventType.Idling, Timestamp = start };
            history.Timestamp = history.Timestamp > start ? history.Timestamp : start;

            return history;
        }

        private async Task<List<EventHistorySummary>> GetEventHistorySummaries(long userId, DateTime start, DateTime end)
        {
            var summaries = await EventUnitOfWork.EventHistorySummary.GetSummaries(userId, start, end).ConfigureAwait(false);

            if (!summaries.Any() || summaries[0].Timestamp != start)
            {
                var previous = await EventUnitOfWork.EventHistorySummary.GetLastSummary(userId, start).ConfigureAwait(false);
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
