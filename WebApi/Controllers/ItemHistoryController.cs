using Core.Interfaces.Services;
using Core.Models.Interruption;
using Core.Models.Task;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace WebApi.Controllers
{
    [Route("api/v1/item-histories")]
    [ApiController]
    public class ItemHistoryController : ControllerBase
    {
        private IItemHistoryService ItemHistoryService { get; }

        public ItemHistoryController(IItemHistoryService itemHistoryService)
        {
            ItemHistoryService = itemHistoryService;
        }

        [HttpPost]
        [Route("idling-sessions")]
        public async Task<bool> StartIdlingSession()
        {
            return await ItemHistoryService.StartIdlingSession().ConfigureAwait(false);
        }

        [HttpPost]
        [Route("interruption-items")]
        public async Task<bool> StartInterruptionItem([FromBody]InterruptionItem item)
        {
            return await ItemHistoryService.StartInterruptionItem(item).ConfigureAwait(false);
        }

        [HttpPost]
        [Route("task-items")]
        public async Task<bool> StartTaskItem([FromBody]TaskItem item)
        {
            return await ItemHistoryService.StartTaskItem(item).ConfigureAwait(false);
        }
    }
}
