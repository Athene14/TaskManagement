using TaskManagementServices.Shared.Exceptions;
using TaskManagementServices.Shared.TaskService;
using TaskManagementServices.Shared.TaskService.DTO;
using TaskService.App.Exceptions;
using TaskService.Domain.Abstractions;
using TaskService.Domain.Models;

namespace TaskService.App.Service
{
    internal class TaskService : ITaskService
    {
        private readonly ITaskRepository _taskRepo;
        private readonly ITaskHistoryRepository _historyRepo;

        public TaskService(
            ITaskRepository taskRepository,
            ITaskHistoryRepository taskHistoryRepository)
        {
            _taskRepo = taskRepository;
            _historyRepo = taskHistoryRepository;
        }

        public async Task<TaskResponse> GetTaskByIdAsync(Guid taskId)
        {
            if (taskId == Guid.Empty)
                throw new InvalidArgumentException("Task is is required");
            var task = await _taskRepo.GetTaskByIdAsync(taskId);
            return task == null ? throw new NotFoundException($"{task} task is not found") : DomainToResponseModel(task);
        }

        public async Task<PagedResponse<TaskResponse>> GetWithFilterAsync(TaskFilter filter, int page, int pageSize)
        {
            var resp = await _taskRepo.GetTasksAsync(filter, page, pageSize);

            return new PagedResponse<TaskResponse>(resp.Items.Select(t => DomainToResponseModel(t)).ToList(),
                                                resp.Page,
                                                resp.PageSize,
                                                resp.TotalCount);
        }

        public async Task<IEnumerable<TaskSnapshotResponse>> GetTaskHistoryAsync(Guid taskId)
        {
            var snapshots = await _historyRepo.GetSnapshotsByTaskIdAsync(taskId);
            if (snapshots == null || !snapshots.Any())
                throw new NotFoundException($"No history for {taskId} found");
            return snapshots.Select(s => new TaskSnapshotResponse
            {
                SnapshotId = s.SnapshotId,
                Title = s.Title,
                Description = s.Description,
                AssignedUserId = s.AssignedUserId,
                IsActive = s.IsActive,
                ChangedBy = s.ChangedBy,
                UpdatedAt = DateTimeOffset.FromUnixTimeSeconds(s.ChangeTime).DateTime,
            });
        }

        public async Task<TaskResponse> CreateTaskAsync(Guid initiatorUserId, CreateTaskRequest request)
        {
            if (request == null) throw new InvalidArgumentException(nameof(request));

            var task = RequestToDomainModel(initiatorUserId, request);

            var taskId = await _taskRepo.CreateTaskAsync(task);
            if (taskId == Guid.Empty)
                throw new InvalidOperationException("Failed to create task");

            await CreateHistorySnapshot(taskId, initiatorUserId, task);

            return DomainToResponseModel(task);
        }

        public async Task<TaskResponse> UpdateTaskAsync(Guid initiatorUserId, Guid taskId, UpdateTaskRequest request)
        {
            if (taskId == Guid.Empty)
                throw new InvalidArgumentException("Task ID must be provided for update");

            var existingTask = await _taskRepo.GetTaskByIdAsync(taskId);
            if (existingTask == null)
                throw new NotFoundException($"Task {taskId} is not found");

            if (!existingTask.IsActive)
                throw new InactiveUpdateTaskException("Can't update not active task");

            if (string.IsNullOrWhiteSpace(request.Title) &&
                string.IsNullOrEmpty(request.Description) &&
                (!request.AssignedUserId.HasValue || request.AssignedUserId == Guid.Empty))
                throw new InvalidArgumentException("Update request must have at least 1 field");

            var needToUpdate = false;
            // проверка на надобность в обновлении
            // обновляем только те поля, который отличаются от модели в базе
            if (!string.IsNullOrEmpty(request.Title) && existingTask.Title != request.Title)
            {
                existingTask.Title = request.Title;
                needToUpdate = true;
            }

            if (request.Description != null && existingTask.Description != request.Description)
            {
                existingTask.Description = request.Description;
                needToUpdate = true;
            }

            if (request.AssignedUserId.HasValue &&
                request.AssignedUserId.Value != Guid.Empty &&
                existingTask.AssignedUserId != request.AssignedUserId.Value)
            {
                existingTask.AssignedUserId = request.AssignedUserId.Value;
                needToUpdate = true;
            }
            //----------------------//

            if (needToUpdate)
            {
                existingTask.UpdatedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

                var success = await _taskRepo.UpdateTaskAsync(existingTask);
                if (!success)
                    throw new InvalidOperationException("Task update failed");

                await CreateHistorySnapshot(taskId, initiatorUserId, existingTask);
            }
            return DomainToResponseModel(existingTask);
        }

        public async Task<bool> DeleteTaskAsync(Guid initiatorUserId, Guid taskId)
        {
            // проверка, что задача существует
            var taskToDelete = await _taskRepo.GetTaskByIdAsync(taskId);
            if (taskToDelete == null)
                throw new NotFoundException($"Task {taskId} is not found");

            if (!taskToDelete.IsActive)
                throw new InactiveUpdateTaskException($"Task {taskId} is not active");

            // Мягкое удаление задачи
            var result = await _taskRepo.SoftDeleteTaskAsync(taskId);
            if (!result)
                // при удалении что-то пошло не так
                throw new Exception($"Active {taskId} is not found");

            taskToDelete.IsActive = false;
            // Фиксация снапшота после удаления
            await CreateHistorySnapshot(taskId, initiatorUserId, taskToDelete);
            return true;
        }


        private TaskDomainModel RequestToDomainModel(Guid initiatorUserId, CreateTaskRequest request)
        {
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            return new TaskDomainModel()
            {
                TaskId = Guid.NewGuid(),
                Title = request.Title,
                Description = request.Description,
                AssignedUserId = request.AssignedUserId,
                IsActive = true,
                CreatedAt = timestamp,
                UpdatedAt = timestamp,
                CreatedBy = initiatorUserId,
            };
        }

        private TaskResponse DomainToResponseModel(TaskDomainModel model)
        {
            return new TaskResponse
            {
                Id = model.TaskId,
                Title = model.Title,
                Description = model.Description,
                AssignedUserId = model.AssignedUserId,
                IsActive = model.IsActive,
                CreatedAt = DateTimeOffset.FromUnixTimeSeconds(model.CreatedAt).DateTime,
                UpdatedAt = DateTimeOffset.FromUnixTimeSeconds(model.UpdatedAt).DateTime,
                CreatedBy = model.CreatedBy
            };
        }

        private async Task CreateHistorySnapshot(
            Guid taskId,
            Guid initiatorUserId,
            TaskDomainModel task)
        {
            await _historyRepo.AddSnapshotAsync(new TaskSnapshotDomainModel
            {
                SnapshotId = Guid.NewGuid(),
                TaskId = taskId,
                Title = task.Title,
                Description = task.Description,
                AssignedUserId = task.AssignedUserId,
                IsActive = task.IsActive,
                ChangeTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                ChangedBy = initiatorUserId
            });
        }


    }
}
