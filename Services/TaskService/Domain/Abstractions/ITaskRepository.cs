using TaskManagementServices.Shared.TaskService.DTO;
using TaskService.Domain.Models;

namespace TaskService.Domain.Abstractions
{
    public interface ITaskRepository
    {
        Task<PagedResponse<TaskDomainModel>> GetTasksAsync(TaskFilter filter, int page, int pageSize);
        Task<TaskDomainModel> GetTaskByIdAsync(Guid taskId);
        Task<Guid> CreateTaskAsync(TaskDomainModel task);
        Task<bool> UpdateTaskAsync(TaskDomainModel task);
        Task<bool> SoftDeleteTaskAsync(Guid taskId);
    }
}
