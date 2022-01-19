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
        public async Task<List<InterruptionItemSummaryDto>> GetItemSummaries()
        {
            return await InterruptionItemRepository.GetItemSummaries().ConfigureAwait(false);
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<InterruptionItem> GetItemById(long id)
        {
            return await InterruptionItemRepository.GetItemById(id).ConfigureAwait(false);
        }

        [HttpPost]
        [Route("")]
        public async Task<InterruptionItem> CreateItem([FromBody]InterruptionItemCreationDto item)
        {
            if (string.IsNullOrWhiteSpace(item.Name))
            {
                return null;
            }

            return await InterruptionItemRepository.CreateItem(item).ConfigureAwait(false);
        }

        [HttpPut]
        [Route("")]
        public async Task<InterruptionItem> UpdateItem([FromBody]InterruptionItem item)
        {
            if (string.IsNullOrWhiteSpace(item.Name) || item.Id < 0)
            {
                return null;
            }

            return await InterruptionItemRepository.UpdateItem(item).ConfigureAwait(false);
        }

        [HttpDelete]
        [Route("{id}")]
        public async Task<bool> DeleteItemById(long id)
        {
            return await InterruptionItemRepository.DeleteItemById(id).ConfigureAwait(false);
        }
    }
}
