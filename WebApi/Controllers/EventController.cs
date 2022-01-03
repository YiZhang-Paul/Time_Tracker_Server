using Core.Interfaces.Services;
using Core.Models.EventHistory;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace WebApi.Controllers
{
    [Route("api/v1/events")]
    [ApiController]
    public class EventController : ControllerBase
    {
        private IEventHistoryService EventHistoryService { get; }

        public EventController(IEventHistoryService eventHistoryService)
        {
            EventHistoryService = eventHistoryService;
        }

        [HttpGet]
        [Route("time-distribution/{start}")]
        public async Task<EventTimeDistribution> GetOngoingTimeDistribution(DateTime start)
        {
            return await EventHistoryService.GetOngoingTimeDistribution(start).ConfigureAwait(false);
        }

        [HttpPost]
        [Route("idling-sessions")]
        public async Task<bool> StartIdlingSession()
        {
            return await EventHistoryService.StartIdlingSession().ConfigureAwait(false);
        }

        [HttpPost]
        [Route("interruption-items/{id}")]
        public async Task<bool> StartInterruptionItem(long id)
        {
            return await EventHistoryService.StartInterruptionItem(id).ConfigureAwait(false);
        }

        [HttpPost]
        [Route("task-items/{id}")]
        public async Task<bool> StartTaskItem(long id)
        {
            return await EventHistoryService.StartTaskItem(id).ConfigureAwait(false);
        }
    }
}
