using Core.Dtos;
using Core.Enums;
using Core.Interfaces.Services;
using Core.Interfaces.UnitOfWorks;
using Core.Models.WorkItem;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace WebApi.Controllers
{
    [Route("api/v1/task-items")]
    [ApiController]
    public class TaskItemController : ControllerBase
    {
        private IWorkItemUnitOfWork WorkItemUnitOfWork { get; }
        private ITaskItemService TaskItemService { get; }

        public TaskItemController(IWorkItemUnitOfWork workItemUnitOfWork, ITaskItemService taskItemService)
        {
            WorkItemUnitOfWork = workItemUnitOfWork;
            TaskItemService = taskItemService;
        }

        [HttpGet]
        [Route("summaries")]
        public async Task<IActionResult> GetItemSummaries([FromQuery]string searchText)
        {
            try
            {
                return Ok(await TaskItemService.GetItemSummaries(searchText).ConfigureAwait(false));
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
            return await TaskItemService.GetItemSummaries(start).ConfigureAwait(false);
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<TaskItem> GetItemById(long id)
        {
            return await WorkItemUnitOfWork.TaskItem.GetItemById(id).ConfigureAwait(false);
        }

        [HttpPost]
        [Route("")]
        public async Task<IActionResult> CreateItem([FromBody]TaskItemBase item)
        {
            try
            {
                return Ok(await TaskItemService.CreateItem(item).ConfigureAwait(false));
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
                return Ok(await TaskItemService.UpdateItem(item, resolve).ConfigureAwait(false));
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
            return await WorkItemUnitOfWork.TaskItem.DeleteItemById(id).ConfigureAwait(false) && await WorkItemUnitOfWork.Save().ConfigureAwait(false);
        }
    }
}
