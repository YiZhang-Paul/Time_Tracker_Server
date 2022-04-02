using Core.Dtos;
using Core.Enums;
using Core.Interfaces.Repositories;
using Core.Interfaces.Services;
using Core.Interfaces.UnitOfWorks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Text;
using System.Threading.Tasks;

namespace WebApi.Controllers
{
    [Route("api/v1/events")]
    [Authorize(Policy = "UserProfile")]
    [ApiController]
    public class EventController : ControllerBase
    {
        private IWorkItemUnitOfWork WorkItemUnitOfWork { get; }
        private IUserService UserService { get; }
        private IInterruptionItemService InterruptionItemService { get; }
        private ITaskItemService TaskItemService { get; }
        private IEventSummaryService EventSummaryService { get; }
        private IEventTrackingService EventTrackingService { get; }

        public EventController
        (
            IWorkItemUnitOfWork workItemUnitOfWork,
            IUserService userService,
            IInterruptionItemService interruptionItemService,
            ITaskItemService taskItemService,
            IEventSummaryService eventSummaryService,
            IEventTrackingService eventTrackingService
        )
        {
            WorkItemUnitOfWork = workItemUnitOfWork;
            UserService = userService;
            InterruptionItemService = interruptionItemService;
            TaskItemService = taskItemService;
            EventSummaryService = eventSummaryService;
            EventTrackingService = eventTrackingService;
        }

        [HttpGet]
        [Route("time-summary/{start}")]
        public async Task<OngoingEventTimeSummaryDto> GetOngoingTimeSummary(DateTime start)
        {
            var user = await UserService.GetProfile(HttpContext.User).ConfigureAwait(false);

            return await EventSummaryService.GetOngoingTimeSummary(user.Id, start).ConfigureAwait(false);
        }

        [HttpGet]
        [Route("event-summaries/{start}")]
        public async Task<EventSummariesDto> GetEventSummariesByDay(DateTime start)
        {
            var user = await UserService.GetProfile(HttpContext.User).ConfigureAwait(false);

            return await EventSummaryService.GetEventSummariesByDay(user.Id, start).ConfigureAwait(false);
        }

        [HttpGet]
        [Route("timesheets/{start}")]
        public async Task<IActionResult> GetTimesheetsByDay(DateTime start)
        {
            var user = await UserService.GetProfile(HttpContext.User).ConfigureAwait(false);
            var timesheets = await EventSummaryService.GetTimesheetsByDay(user.Id, start).ConfigureAwait(false);
            var file = Encoding.UTF8.GetBytes(string.Join("\n", timesheets));

            return File(file, "text/plain", $"timesheets_{start:yyyy_MM_dd}");
        }

        [HttpPost]
        [Route("idling-sessions")]
        public async Task<bool> StartIdlingSession()
        {
            var user = await UserService.GetProfile(HttpContext.User).ConfigureAwait(false);

            return await EventTrackingService.StartIdlingSession(user.Id).ConfigureAwait(false);
        }

        [HttpPost]
        [Route("interruption-items/{id}")]
        public async Task<bool> StartInterruptionItem(long id)
        {
            var user = await UserService.GetProfile(HttpContext.User).ConfigureAwait(false);
            var item = await WorkItemUnitOfWork.InterruptionItem.GetItemById(user.Id, id).ConfigureAwait(false);

            if (item == null)
            {
                return false;
            }

            if (item.ResolvedTime != null && await InterruptionItemService.UpdateItem(item, ResolveAction.Unresolve).ConfigureAwait(false) == null)
            {
                return false;
            }

            return await EventTrackingService.StartInterruptionItem(user.Id, id).ConfigureAwait(false);
        }

        [HttpPost]
        [Route("task-items/{id}")]
        public async Task<bool> StartTaskItem(long id)
        {
            var user = await UserService.GetProfile(HttpContext.User).ConfigureAwait(false);
            var item = await WorkItemUnitOfWork.TaskItem.GetItemById(user.Id, id).ConfigureAwait(false);

            if (item == null)
            {
                return false;
            }

            if (item.ResolvedTime != null && await TaskItemService.UpdateItem(user.Id, item, ResolveAction.Unresolve).ConfigureAwait(false) == null)
            {
                return false;
            }

            return await EventTrackingService.StartTaskItem(user.Id, id).ConfigureAwait(false);
        }

        [HttpPost]
        [Route("scheduled-break-prompts")]
        public async Task<IActionResult> ConfirmBreakSessionPrompt([FromBody]BreakSessionConfirmationDto confirmation)
        {
            try
            {
                var user = await UserService.GetProfile(HttpContext.User).ConfigureAwait(false);

                if (confirmation.IsSkip)
                {
                    return Ok(await EventTrackingService.SkipBreakSession(user.Id).ConfigureAwait(false));
                }

                return Ok(await EventTrackingService.StartBreakSession(user.Id, confirmation.TargetDuration).ConfigureAwait(false));
            }
            catch (ArgumentException exception)
            {
                return BadRequest(exception.Message);
            }
        }

        [HttpPut]
        [Route("time-range")]
        public async Task<IActionResult> UpdateTimeRange([FromBody]EventTimeRangeDto range)
        {
            try
            {
                var user = await UserService.GetProfile(HttpContext.User).ConfigureAwait(false);

                return Ok(await EventTrackingService.UpdateTimeRange(user.Id, range).ConfigureAwait(false));
            }
            catch (ArgumentException exception)
            {
                return BadRequest(exception.Message);
            }
        }
    }
}
