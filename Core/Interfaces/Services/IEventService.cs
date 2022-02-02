using Core.Dtos;
using Core.Models.Event;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Interfaces.Services
{
    public interface IEventService
    {
        Task<OngoingEventTimeSummaryDto> GetOngoingTimeSummary(DateTime start);
        Task<EventSummariesDto> GetEventSummariesByDay(DateTime start);
        Task<EventTimeBreakdownDto> GetTimeBreakdownByDay(DateTime start);
        Task<List<EventHistorySummary>> GetEventHistorySummariesByDay(DateTime start);
        Task<bool> StartIdlingSession();
        Task<bool> StartInterruptionItem(long id);
        Task<bool> StartTaskItem(long id);
        Task<bool> StartBreakSession(int duration);
        Task<bool> SkipBreakSession();
    }
}
