using Core.Dtos;
using Core.Interfaces.Repositories;
using Core.Models.Interruption;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WebApi.Controllers
{
    [Route("api/v1/interruption-items")]
    [ApiController]
    public class InterruptionItemController : ControllerBase
    {
        private IInterruptionItemRepository InterruptionItemRepository { get; }

        public InterruptionItemController(IInterruptionItemRepository interruptionItemRepository)
        {
            InterruptionItemRepository = interruptionItemRepository;
        }

        [HttpGet]
        [Route("summaries")]
        public async Task<List<InterruptionItemSummaryDto>> GetInterruptionItemSummaries()
        {
            return await InterruptionItemRepository.GetInterruptionItemSummaries().ConfigureAwait(false);
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<InterruptionItem> GetInterruptionItemById(long id)
        {
            return await InterruptionItemRepository.GetInterruptionItemById(id).ConfigureAwait(false);
        }

        [HttpPost]
        [Route("")]
        public async Task<InterruptionItem> CreateInterruptionItem([FromBody]InterruptionItemCreationDto item)
        {
            if (string.IsNullOrWhiteSpace(item.Name))
            {
                return null;
            }

            return await InterruptionItemRepository.CreateInterruptionItem(item).ConfigureAwait(false);
        }
    }
}
