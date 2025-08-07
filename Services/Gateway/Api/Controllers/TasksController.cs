using Gateway.Domain.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Security.Claims;
using System.Text;
using TaskManagementServices.Shared.TaskService;
using TaskManagementServices.Shared.TaskService.DTO;

namespace Gateway.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/tasks")]
    [Produces("application/json")]
    public class TasksController : ControllerBase
    {
        private readonly ITaskService _taskService;
        private readonly IMemoryCache _cache;

        public TasksController(ITaskService taskService, IMemoryCache cache)
        {
            _taskService = taskService;
            _cache = cache;
        }

        // Вспомогательные методы для генерации ключей кэша
        private string GetTaskCacheKey(Guid taskId) => $"task_{taskId}";
        private string GetTasksCacheKey(TaskFilter filter, int page, int pageSize) =>
            $"tasks_{GetFilterKey(filter)}_p{page}_s{pageSize}";

        private string GetFilterKey(TaskFilter filter)
        {
            StringBuilder builder = new StringBuilder();
            if (!string.IsNullOrEmpty(filter.Title)) builder.Append(filter.Title + "_");
            if (filter.AssignedUserId.HasValue) builder.Append(filter.AssignedUserId.Value + "_");
            if (filter.CreatedFromTimestamp.HasValue) builder.Append(filter.CreatedFromTimestamp.Value);
            if (filter.CreatedToTimestamp.HasValue) builder.Append(filter.CreatedToTimestamp.Value + "_");
            if (filter.CreatedBy.HasValue) builder.Append(filter.CreatedBy.Value + "_");
            if (filter.OnlyActive.HasValue && filter.OnlyActive.Value) builder.Append("active");

            return builder.ToString();
        }

        // существует только для поддержки кэша с фильтрами
        private string GetTaskHistoryCacheKey(Guid taskId) => $"task_history_{taskId}";

        // Метод для инвалидации всех связанных с задачей кэшей
        private void InvalidateTaskCache(Guid taskId)
        {
            _cache.Remove(GetTaskCacheKey(taskId));
            _cache.Remove(GetTaskHistoryCacheKey(taskId));
            InvalidateTasksListCache();
        }
        private void InvalidateTasksListCache()
        {
            _cache.Set("tasks_list_version", (_cache.Get<int?>("tasks_list_version") ?? 0) + 1,
                new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromHours(24)));
        }

        [HttpGet]
        [SwaggerResponse((int)HttpStatusCode.OK, "Tasks", typeof(PagedResponse<TaskResponse>))]
        public async Task<IActionResult> GetTasks(
            [FromQuery] TaskFilter filter,
            [FromQuery, Range(1, int.MaxValue)] int page = 1,
            [FromQuery, Range(1, 100)] int pageSize = 10)
        {
            var initiatorUserId = GetCurrentUserId();

            // Получаем глобальную версию кэша списков
            var cacheVersion = _cache.GetOrCreate("tasks_list_version", entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24);
                return 0;
            });

            var cacheKey = $"{GetTasksCacheKey(filter, page, pageSize)}_v{cacheVersion}";

            if (_cache.TryGetValue(cacheKey, out PagedResponse<TaskResponse>? result))
            {
                return Ok(result);
            }

            result = await _taskService.GetWithFilterAsync(filter, page, pageSize);

            // Кэшируем на 5 минут + инвалидация через глобальную версию
            _cache.Set(cacheKey, result, TimeSpan.FromMinutes(5));

            return Ok(result);
        }

        // добавил эндпоинт, которого нет в тз, но его функционал подразумевался в JobHistory
        [HttpGet("{taskId:guid}/history")]
        [SwaggerResponse((int)HttpStatusCode.OK, "Task history", typeof(List<TaskResponse>))]
        public async Task<ActionResult<IEnumerable<TaskSnapshotResponse>>> GetTaskHistoryAsync(Guid taskId)
        {
            var initiatorUserId = GetCurrentUserId();

            var cacheKey = GetTaskHistoryCacheKey(taskId);

            if (_cache.TryGetValue(cacheKey, out List<TaskSnapshotResponse>? result))
            {
                return Ok(result);
            }

            result = (await _taskService.GetTaskHistoryAsync(taskId)).ToList();

            // Кэшируем историю на 10 минут
            _cache.Set(cacheKey, result, TimeSpan.FromMinutes(10));

            return Ok(result);
        }

        [HttpGet("{taskId:guid}")]
        [SwaggerResponse((int)HttpStatusCode.OK, "Task", typeof(TaskResponse))]
        public async Task<IActionResult> GetTaskById(Guid taskId)
        {
            var initiatorUserId = GetCurrentUserId();

            var cacheKey = GetTaskCacheKey(taskId);

            if (_cache.TryGetValue(cacheKey, out TaskResponse? task))
            {
                return Ok(task);
            }

            task = await _taskService.GetTaskByIdAsync(taskId);
            if (task == null)
                return NotFound();

            // Кэшируем задачу на 15 минут
            _cache.Set(cacheKey, task, TimeSpan.FromMinutes(15));

            return Ok(task);
        }

        [HttpPost]
        [SwaggerResponse((int)HttpStatusCode.Created, "Created task", typeof(TaskResponse))]
        public async Task<IActionResult> CreateTask(CreateTaskRequest request)
        {
            var initiatorUserId = GetCurrentUserId();
            var createdTask = await _taskService.CreateTaskAsync(initiatorUserId, request);

            // Инвалидация через увеличение версии вместо удаления
            InvalidateTasksListCache();

            return CreatedAtAction(nameof(CreateTask), createdTask);
        }

        [HttpPut("{taskId:guid}")]
        [SwaggerResponse((int)HttpStatusCode.OK, "Updated task", typeof(TaskResponse))]
        public async Task<IActionResult> UpdateTask(Guid taskId, UpdateTaskRequest request)
        {
            var initiatorUserId = GetCurrentUserId();

            var updatedTask = await _taskService.UpdateTaskAsync(initiatorUserId, taskId, request);
            if (updatedTask == null)
                return NotFound();

            // Инвалидируем кэши для этой задачи
            InvalidateTaskCache(taskId);

            return Ok(updatedTask);
        }

        [HttpDelete("{taskId:guid}")]
        public async Task<IActionResult> DeleteTask(Guid taskId)
        {
            var initiatorUserId = GetCurrentUserId();

            var result = await _taskService.DeleteTaskAsync(initiatorUserId, taskId);
            if (!result)
                return NotFound();

            // Инвалидируем кэши для удаленной задачи
            InvalidateTaskCache(taskId);

            return Ok();
        }

        [HttpPut("{taskId:guid}/assign")]
        [SwaggerResponse((int)HttpStatusCode.OK, "Updated Task", typeof(TaskResponse))]
        public async Task<IActionResult> AssignTask(Guid taskId, AssignTaskRequest request)
        {
            var initiatorUserId = GetCurrentUserId();

            var currentTask = await _taskService.GetTaskByIdAsync(taskId);
            if (currentTask == null)
                return NotFound();

            var updateRequest = new UpdateTaskRequest
            {
                Title = currentTask.Title,
                Description = currentTask.Description,
                AssignedUserId = request.AssignedUserId
            };

            var updatedTask = await _taskService.UpdateTaskAsync(initiatorUserId, taskId, updateRequest);

            InvalidateTaskCache(taskId);

            return Ok(updatedTask);
        }

        // Вспомогательный метод для получения текущего UserId из JWT
        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out Guid userId))
            {
                throw new UnauthorizedAccessException("Invalid user identifier");
            }
            return userId;
        }
    }

}

