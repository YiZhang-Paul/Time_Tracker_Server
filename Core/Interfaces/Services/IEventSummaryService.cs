using Core.Dtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Interfaces.Services
{
    public interface IEventSummaryService
    {
        Task<OngoingEventTimeSummaryDto> GetOngoingTimeSummary(long userId, DateTime start);
        Task<EventSummariesDto> GetEventSummariesByDay(long userId, DateTime start);
        Task<List<string>> GetTimesheetsByDay(long userId, DateTime start);
    }
}
