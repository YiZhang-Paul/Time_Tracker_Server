using Core.Interfaces.Services;
using Core.Models.Interruption;
using Core.Models.Task;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace WebApi.Controllers
{
    [Route("api/v1/event-histories")]
    [ApiController]
    public class EventHistoryController : ControllerBase
    {
        private IEventHistoryService EventHistoryService { get; }

        public EventHistoryController(IEventHistoryService eventHistoryService)
        {
            EventHistoryService = eventHistoryService;
        }

        [HttpPost]
        [Route("idling-sessions")]
        public async Task<bool> StartIdlingSession()
        {
            return await EventHistoryService.StartIdlingSession().ConfigureAwait(false);
        }

        [HttpPost]
        [Route("interruption-items")]
        public async Task<bool> StartInterruptionItem([FromBody]InterruptionItem item)
        {
            return await EventHistoryService.StartInterruptionItem(item).ConfigureAwait(false);
        }

        [HttpPost]
        [Route("task-items")]
        public async Task<bool> StartTaskItem([FromBody]TaskItem item)
        {
            return await EventHistoryService.StartTaskItem(item).ConfigureAwait(false);
        }
    }
}
