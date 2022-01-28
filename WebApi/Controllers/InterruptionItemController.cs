using Core.Dtos;
using Core.Enums;
using Core.Interfaces.Repositories;
using Core.Models.Interruption;
using Microsoft.AspNetCore.Mvc;
using System;
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
        public async Task<IActionResult> CreateItem([FromBody]InterruptionItemCreationDto item)
        {
            if (string.IsNullOrWhiteSpace(item.Name))
            {
                return BadRequest("Name must not be null.");
            }

            return Ok(await InterruptionItemRepository.CreateItem(item).ConfigureAwait(false));
        }

        [HttpPut]
        [Route("")]
        public async Task<IActionResult> UpdateItem([FromBody]InterruptionItem item, [FromQuery]ResolveAction resolve = ResolveAction.None)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(item.Name) || item.Id < 0)
                {
                    return BadRequest("Name must not be null.");
                }

                return Ok(await InterruptionItemRepository.UpdateItem(item, resolve).ConfigureAwait(false));
            }
            catch (ArgumentException exception)
            {
                return BadRequest(exception.Message);
            }
        }

        [HttpDelete]
        [Route("{id}")]
        public async Task<bool> DeleteItemById(long id)
        {
            return await InterruptionItemRepository.DeleteItemById(id).ConfigureAwait(false);
        }
    }
}
