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
        public async Task<List<TaskItemSummaryDto>> GetTaskItemSummaries()
        {
            return await TaskItemRepository.GetTaskItemSummaries().ConfigureAwait(false);
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<TaskItem> GetTaskItemById(long id)
        {
            return await TaskItemRepository.GetTaskItemById(id).ConfigureAwait(false);
        }

        [HttpPost]
        [Route("")]
        public async Task<TaskItem> CreateTaskItem([FromBody]TaskItemCreationDto item)
        {
            if (string.IsNullOrWhiteSpace(item.Name))
            {
                return null;
            }

            return await TaskItemRepository.CreateTaskItem(item).ConfigureAwait(false);
        }
    }
}
