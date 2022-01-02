using Core.Interfaces.Repositories;
using Core.Interfaces.Services;
using Core.Models.EventHistory;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace WebApi.Controllers
{
    [Route("api/v1/event-histories")]
    [ApiController]
    public class EventHistoryController : ControllerBase
    {
        private IEventHistoryRepository EventHistoryRepository { get; }
        private IEventHistoryService EventHistoryService { get; }

        public EventHistoryController(IEventHistoryRepository eventHistoryRepository, IEventHistoryService eventHistoryService)
        {
            EventHistoryRepository = eventHistoryRepository;
            EventHistoryService = eventHistoryService;
        }

        [HttpGet]
        [Route("last-history")]
        public async Task<EventHistory> GetLastEventHistory()
        {
            return await EventHistoryRepository.GetLastEventHistory().ConfigureAwait(false);
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
