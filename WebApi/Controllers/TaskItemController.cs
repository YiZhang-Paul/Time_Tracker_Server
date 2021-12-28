using Core.Dtos;
using Core.Interfaces.Repositories;
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
        public async Task<List<TaskItemDto>> GetTaskItemSummaries()
        {
            return await TaskItemRepository.GetTaskItemSummaries().ConfigureAwait(false);
        }
    }
}
