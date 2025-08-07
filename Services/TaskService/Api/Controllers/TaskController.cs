using Microsoft.AspNetCore.Mvc;
using TaskManagementServices.Shared.TaskService;
using TaskManagementServices.Shared.TaskService.DTO;

namespace TaskService.Api.Controllers
{
    [Route("")]
    [ApiController]
    public class TasksController : ControllerBase
    {
        private readonly ITaskService _taskService;
        private readonly ILogger<TasksController> _log;

        public TasksController(ILogger<TasksController> log, ITaskService taskService)
        {
            _taskService = taskService;
            _log = log;
        }

        [HttpGet("{taskId}")]
        public async Task<ActionResult<TaskResponse>> GetTaskById(
            [FromRoute] Guid taskId)
        {
            if (taskId == Guid.Empty)
            {
                return BadRequest("Invalid task identifier");
            }

            var response = await _taskService.GetTaskByIdAsync(taskId);
            return Ok(response);
        }

        [HttpGet]
        public async Task<ActionResult<PagedResponse<TaskResponse>>> GetWithFilter(
            [FromQuery] TaskFilter filter,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            if (page < 1 || pageSize < 1 || pageSize > 100)
            {
                _log.LogWarning($"Invalid pagination: page={page}, pageSize={pageSize}");
                return BadRequest("Page and pageSize must be positive (max 100)");
            }

            var response = await _taskService.GetWithFilterAsync(filter, page, pageSize);
            return Ok(response);
        }

        [HttpGet("{taskId}/history")]
        public async Task<ActionResult<IEnumerable<TaskSnapshotResponse>>> GetTaskHistory(
            [FromRoute] Guid taskId)
        {
            if (taskId == Guid.Empty)
            {
                return BadRequest("Invalid task identifier");
            }

            var history = await _taskService.GetTaskHistoryAsync(taskId);
            return history.Any() ? Ok(history) : NotFound();
        }

        [HttpPost]
        public async Task<ActionResult<TaskResponse>> CreateTask(
            [FromQuery] Guid initiatorUserId,
            [FromBody] CreateTaskRequest request)
        {
            if (initiatorUserId == Guid.Empty)
            {
                return BadRequest("Initiator ID is required");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _taskService.CreateTaskAsync(initiatorUserId, request);
            return Ok(response);
        }

        [HttpPut("{taskId}")]
        public async Task<ActionResult<TaskResponse>> UpdateTask(
            [FromQuery] Guid initiatorUserId,
            [FromRoute] Guid taskId,
            [FromBody] UpdateTaskRequest request)
        {
            if (initiatorUserId == Guid.Empty)
            {
                return BadRequest("Initiator ID is required");
            }

            if (taskId == Guid.Empty)
            {
                return BadRequest("Task ID is required");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _taskService.UpdateTaskAsync(initiatorUserId, taskId, request);
            return response == null ? NotFound() : Ok(response);
        }

        [HttpDelete("{taskId}")]
        public async Task<IActionResult> DeleteTask(
            [FromQuery] Guid initiatorUserId,
            [FromRoute] Guid taskId)
        {
            if (initiatorUserId == Guid.Empty)
            {
                return BadRequest("Initiator ID is required");
            }

            if (taskId == Guid.Empty)
            {
                return BadRequest("Task ID is required");
            }

            var result = await _taskService.DeleteTaskAsync(initiatorUserId, taskId);
            return result ? NoContent() : NotFound();
        }

    }
}
