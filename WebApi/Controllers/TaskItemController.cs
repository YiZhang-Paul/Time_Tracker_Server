using Core.Dtos;
using Core.Interfaces.Repositories;
using Core.Models.Task;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WebApi.Controllers
{
    [Route("api/v1/task-items")]
    [ApiController]
    public class TaskItemController : ControllerBase
    {
        private ITaskItemRepository TaskItemRepository { get; }

        public TaskItemController(ITaskItemRepository taskItemRepository)
        {
            TaskItemRepository = taskItemRepository;
        }

        [HttpGet]
        [Route("summaries")]
        public async Task<List<TaskItemSummaryDto>> GetItemSummaries()
        {
            return await TaskItemRepository.GetItemSummaries().ConfigureAwait(false);
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
                return BadRequest("Name must not be null.");
            }

            return Ok(await TaskItemRepository.CreateItem(item).ConfigureAwait(false));
        }

        [HttpPut]
        [Route("")]
        public async Task<IActionResult> UpdateItem([FromBody]TaskItem item)
        {
            if (string.IsNullOrWhiteSpace(item.Name) || item.Id < 0)
            {
                return BadRequest("Name must not be null.");
            }

            return Ok(await TaskItemRepository.UpdateItem(item).ConfigureAwait(false));
        }

        [HttpDelete]
        [Route("{id}")]
        public async Task<bool> DeleteItemById(long id)
        {
            return await TaskItemRepository.DeleteItemById(id).ConfigureAwait(false);
        }
    }
}
