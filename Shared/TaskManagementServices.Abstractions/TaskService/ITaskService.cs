using TaskManagementServices.Shared.TaskService.DTO;

namespace TaskManagementServices.Shared.TaskService
{
    public interface ITaskService
    {
        Task<TaskResponse> GetTaskByIdAsync(Guid taskId);
        Task<PagedResponse<TaskResponse>> GetWithFilterAsync(TaskFilter filter, int page, int pageSize);
        Task<IEnumerable<TaskSnapshotResponse>> GetTaskHistoryAsync(Guid taskId);
        Task<TaskResponse> CreateTaskAsync(Guid initiatorUserId, CreateTaskRequest request);
        Task<TaskResponse> UpdateTaskAsync(Guid initiatorUserId, Guid taskId, UpdateTaskRequest request);
        Task<bool> DeleteTaskAsync(Guid initiatorUserId, Guid taskId);
    }
}
