using Core.Dtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Interfaces.Services
{
    public interface IEventSummaryService
    {
        Task<OngoingEventTimeSummaryDto> GetOngoingTimeSummary(DateTime start);
        Task<EventSummariesDto> GetEventSummariesByDay(DateTime start);
        Task<List<string>> GetTimesheetsByDay(DateTime start);
        Task<bool> StartInterruptionItem(long id);
        Task<bool> StartTaskItem(long id);
        Task<bool> StartBreakSession(int duration);
        Task<bool> SkipBreakSession();
        Task<bool> UpdateTimeRange(EventTimeRangeDto range);
    }
}
