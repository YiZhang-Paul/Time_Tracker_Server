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
    [Route("api/v1/task-items")]
    [Authorize(Policy = "UserProfile")]
    [ApiController]
    public class TaskItemController : ControllerBase
    {
        private IWorkItemUnitOfWork WorkItemUnitOfWork { get; }
        private IUserService UserService { get; }
        private ITaskItemService TaskItemService { get; }

        public TaskItemController
        (
            IWorkItemUnitOfWork workItemUnitOfWork,
            IUserService userService,
            ITaskItemService taskItemService
        )
        {
            WorkItemUnitOfWork = workItemUnitOfWork;
            UserService = userService;
            TaskItemService = taskItemService;
        }

        [HttpGet]
        [Route("summaries")]
        public async Task<IActionResult> GetItemSummaries([FromQuery]string searchText)
        {
            try
            {
                var user = await UserService.GetProfile(HttpContext.User).ConfigureAwait(false);

                return Ok(await TaskItemService.GetItemSummaries(user.Id, searchText).ConfigureAwait(false));
            }
            catch (ArgumentException exception)
            {
                return BadRequest(exception.Message);
            }
        }

        [HttpGet]
        [Route("summaries/{start}")]
        public async Task<ItemSummariesDto<TaskItemSummaryDto>> GetItemSummaries(DateTime start)
        {
            var user = await UserService.GetProfile(HttpContext.User).ConfigureAwait(false);

            return await TaskItemService.GetItemSummaries(user.Id, start).ConfigureAwait(false);
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<TaskItem> GetItemById(long id)
        {
            var user = await UserService.GetProfile(HttpContext.User).ConfigureAwait(false);

            return await WorkItemUnitOfWork.TaskItem.GetItemById(user.Id, id).ConfigureAwait(false);
        }

        [HttpPost]
        [Route("")]
        public async Task<IActionResult> CreateItem([FromBody]TaskItemBase item)
        {
            try
            {
                var user = await UserService.GetProfile(HttpContext.User).ConfigureAwait(false);

                return Ok(await TaskItemService.CreateItem(user.Id, item).ConfigureAwait(false));
            }
            catch (ArgumentException exception)
            {
                return BadRequest(exception.Message);
            }
        }

        [HttpPut]
        [Route("")]
        public async Task<IActionResult> UpdateItem([FromBody]TaskItem item, [FromQuery]ResolveAction resolve = ResolveAction.None)
        {
            try
            {
                var user = await UserService.GetProfile(HttpContext.User).ConfigureAwait(false);

                return Ok(await TaskItemService.UpdateItem(user.Id, item, resolve).ConfigureAwait(false));
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

            return await WorkItemUnitOfWork.TaskItem.DeleteItemById(user.Id, id).ConfigureAwait(false) && await WorkItemUnitOfWork.Save().ConfigureAwait(false);
        }
    }
}
