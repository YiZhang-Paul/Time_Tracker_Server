using Core.Dtos;
using Core.Enums;
using Core.Interfaces.Repositories;
using Core.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Text;
using System.Threading.Tasks;

namespace WebApi.Controllers
{
    [Route("api/v1/events")]
    [ApiController]
    public class EventController : ControllerBase
    {
        private IInterruptionItemRepository InterruptionItemRepository { get; }
        private ITaskItemRepository TaskItemRepository { get; }
        private IInterruptionItemService InterruptionItemService { get; }
        private ITaskItemService TaskItemService { get; }
        private IEventService EventService { get; }

        public EventController
        (
            IInterruptionItemRepository interruptionItemRepository,
            ITaskItemRepository taskItemRepository,
            IInterruptionItemService interruptionItemService,
            ITaskItemService taskItemService,
            IEventService eventService
        )
        {
            InterruptionItemRepository = interruptionItemRepository;
            TaskItemRepository = taskItemRepository;
            InterruptionItemService = interruptionItemService;
            TaskItemService = taskItemService;
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
        public async Task<EventSummariesDto> GetEventSummariesByDay(DateTime start)
        {
            return await EventService.GetEventSummariesByDay(start).ConfigureAwait(false);
        }

        [HttpGet]
        [Route("timesheets/{start}")]
        public async Task<IActionResult> GetTimesheetsByDay(DateTime start)
        {
            var timesheets = await EventService.GetTimesheetsByDay(start).ConfigureAwait(false);
            var file = Encoding.UTF8.GetBytes(string.Join("\n", timesheets));

            return File(file, "text/plain", $"timesheets_{DateTime.UtcNow:dd_MM_yyyy}");
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
            var item = await InterruptionItemRepository.GetItemById(id).ConfigureAwait(false);

            if (item == null)
            {
                return false;
            }

            if (item.ResolvedTime != null && await InterruptionItemService.UpdateItem(item, ResolveAction.Unresolve).ConfigureAwait(false) == null)
            {
                return false;
            }

            return await EventService.StartInterruptionItem(id).ConfigureAwait(false);
        }

        [HttpPost]
        [Route("task-items/{id}")]
        public async Task<bool> StartTaskItem(long id)
        {
            var item = await TaskItemRepository.GetItemById(id).ConfigureAwait(false);

            if (item == null)
            {
                return false;
            }

            if (item.ResolvedTime != null && await TaskItemService.UpdateItem(item, ResolveAction.Unresolve).ConfigureAwait(false) == null)
            {
                return false;
            }

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
