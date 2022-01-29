using Core.Dtos;
using Core.Enums;
using Core.Interfaces.Repositories;
using Core.Interfaces.Services;
using Core.Models.Interruption;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace WebApi.Controllers
{
    [Route("api/v1/interruption-items")]
    [ApiController]
    public class InterruptionItemController : ControllerBase
    {
        private IInterruptionItemRepository InterruptionItemRepository { get; }
        private IInterruptionItemService InterruptionItemService { get; }

        public InterruptionItemController(IInterruptionItemRepository interruptionItemRepository, IInterruptionItemService interruptionItemService)
        {
            InterruptionItemRepository = interruptionItemRepository;
            InterruptionItemService = interruptionItemService;
        }

        [HttpGet]
        [Route("summaries/{start}")]
        public async Task<ItemSummariesDto> GetItemSummaries(DateTime start)
        {
            return await InterruptionItemService.GetItemSummaries(start).ConfigureAwait(false);
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
                return BadRequest("Name must not be null or empty.");
            }

            return Ok(await InterruptionItemRepository.CreateItem(item).ConfigureAwait(false));
        }

        [HttpPut]
        [Route("")]
        public async Task<IActionResult> UpdateItem([FromBody]InterruptionItem item, [FromQuery]ResolveAction resolve = ResolveAction.None)
        {
            try
            {
                return Ok(await InterruptionItemService.UpdateItem(item, resolve).ConfigureAwait(false));
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
