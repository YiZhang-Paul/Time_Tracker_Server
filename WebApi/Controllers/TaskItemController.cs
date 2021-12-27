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
        [Route("")]
        public async Task<List<TaskItem>> GetTaskItems()
        {
            return await TaskItemRepository.GetVisibleTaskItems().ConfigureAwait(false);
        }
    }
}
