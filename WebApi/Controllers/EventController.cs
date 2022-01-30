using Core.Dtos;
using Core.Interfaces.Services;
using Core.Models.Event;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WebApi.Controllers
{
    [Route("api/v1/events")]
    [ApiController]
    public class EventController : ControllerBase
    {
        private IEventService EventService { get; }

        public EventController(IEventService eventService)
        {
            EventService = eventService;
        }

        [HttpGet]
        [Route("time-summary/{start}")]
        public async Task<OngoingEventTimeSummaryDto> GetOngoingTimeSummary(DateTime start)
        {
            return await EventService.GetOngoingTimeSummary(start).ConfigureAwait(false);
        }

        [HttpGet]
        [Route("event-summaries/{start}")]
        public async Task<List<EventHistorySummary>> GetEventHistorySummariesByDay(DateTime start)
        {
            return await EventService.GetEventHistorySummariesByDay(start).ConfigureAwait(false);
        }

        [HttpPost]
        [Route("idling-sessions")]
        public async Task<bool> StartIdlingSession()
        {
            return await EventService.StartIdlingSession().ConfigureAwait(false);
        }

        [HttpPost]
        [Route("interruption-items/{id}")]
        public async Task<bool> StartInterruptionItem(long id)
        {
            return await EventService.StartInterruptionItem(id).ConfigureAwait(false);
        }

        [HttpPost]
        [Route("task-items/{id}")]
        public async Task<bool> StartTaskItem(long id)
        {
            return await EventService.StartTaskItem(id).ConfigureAwait(false);
        }

        [HttpPost]
        [Route("scheduled-break-prompts")]
        public async Task<IActionResult> ConfirmBreakSessionPrompt([FromBody]BreakSessionConfirmationDto confirmation)
        {
            try
            {
                if (confirmation.IsSkip)
                {
                    return Ok(await EventService.SkipBreakSession().ConfigureAwait(false));
                }

                return Ok(await EventService.StartBreakSession(confirmation.TargetDuration).ConfigureAwait(false));
            }
            catch (ArgumentException exception)
            {
                return BadRequest(exception.Message);
            }
        }
    }
}
