using Core.Dtos;
using Core.Enums;
using Core.Interfaces.Repositories;
using Core.Interfaces.Services;
using Core.Models.Task;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WebApi.Controllers
{
    [Route("api/v1/task-items")]
    [ApiController]
    public class TaskItemController : ControllerBase
    {
        private ITaskItemRepository TaskItemRepository { get; }
        private ITaskItemService TaskItemService { get; }

        public TaskItemController(ITaskItemRepository taskItemRepository, ITaskItemService taskItemService)
        {
            TaskItemRepository = taskItemRepository;
            TaskItemService = taskItemService;
        }

        [HttpGet]
        [Route("summaries")]
        public async Task<List<TaskItemSummaryDto>> GetItemSummaries()
        {
            return await TaskItemRepository.GetUnresolvedItemSummaries().ConfigureAwait(false);
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<TaskItem> GetItemById(long id)
        {
            return await TaskItemRepository.GetItemById(id).ConfigureAwait(false);
        }

        [HttpPost]
        [Route("")]
        public async Task<IActionResult> CreateItem([FromBody]TaskItemCreationDto item)
        {
            if (string.IsNullOrWhiteSpace(item.Name))
            {
                return BadRequest("Name must not be null or empty.");
            }

            return Ok(await TaskItemRepository.CreateItem(item).ConfigureAwait(false));
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
            return await TaskItemRepository.DeleteItemById(id).ConfigureAwait(false);
        }
    }
}
