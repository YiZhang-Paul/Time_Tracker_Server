using Core.Dtos;
using Core.Enums;
using Core.Interfaces.Services;
using Core.Interfaces.UnitOfWorks;
using Core.Models.WorkItem;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace WebApi.Controllers
{
    [Route("api/v1/interruption-items")]
    [Authorize(Policy = "UserProfile")]
    [ApiController]
    public class InterruptionItemController : ControllerBase
    {
        private IWorkItemUnitOfWork WorkItemUnitOfWork { get; }
        private IUserService UserService { get; }
        private IInterruptionItemService InterruptionItemService { get; }

        public InterruptionItemController
        (
            IWorkItemUnitOfWork workItemUnitOfWork,
            IUserService userService,
            IInterruptionItemService interruptionItemService
        )
        {
            WorkItemUnitOfWork = workItemUnitOfWork;
            UserService = userService;
            InterruptionItemService = interruptionItemService;
        }

        [HttpGet]
        [Route("summaries")]
        public async Task<IActionResult> GetItemSummaries([FromQuery]string searchText)
        {
            try
            {
                var user = await UserService.GetProfile(HttpContext.User).ConfigureAwait(false);

                return Ok(await InterruptionItemService.GetItemSummaries(user.Id, searchText).ConfigureAwait(false));
            }
            catch (ArgumentException exception)
            {
                return BadRequest(exception.Message);
            }
        }

        [HttpGet]
        [Route("summaries/{start}")]
        public async Task<ItemSummariesDto<InterruptionItemSummaryDto>> GetItemSummaries(DateTime start)
        {
            var user = await UserService.GetProfile(HttpContext.User).ConfigureAwait(false);

            return await InterruptionItemService.GetItemSummaries(user.Id, start).ConfigureAwait(false);
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<InterruptionItem> GetItemById(long id)
        {
            var user = await UserService.GetProfile(HttpContext.User).ConfigureAwait(false);

            return await WorkItemUnitOfWork.InterruptionItem.GetItemById(user.Id, id).ConfigureAwait(false);
        }

        [HttpPost]
        [Route("")]
        public async Task<IActionResult> CreateItem([FromBody]InterruptionItemBase item)
        {
            try
            {
                var user = await UserService.GetProfile(HttpContext.User).ConfigureAwait(false);
                item.UserId = user.Id;

                return Ok(await InterruptionItemService.CreateItem(item).ConfigureAwait(false));
            }
            catch (ArgumentException exception)
            {
                return BadRequest(exception.Message);
            }
        }

        [HttpPut]
        [Route("")]
        public async Task<IActionResult> UpdateItem([FromBody]InterruptionItem item, [FromQuery]ResolveAction resolve = ResolveAction.None)
        {
            try
            {
                var user = await UserService.GetProfile(HttpContext.User).ConfigureAwait(false);
                item.UserId = user.Id;

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
            var user = await UserService.GetProfile(HttpContext.User).ConfigureAwait(false);

            return await WorkItemUnitOfWork.InterruptionItem.DeleteItemById(user.Id, id).ConfigureAwait(false) && await WorkItemUnitOfWork.Save().ConfigureAwait(false);
        }
    }
}
