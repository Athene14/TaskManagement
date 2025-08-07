using TaskService.Domain.Models;

namespace TaskService.Domain.Abstractions
{
    public interface ITaskHistoryRepository
    {
        Task AddSnapshotAsync(TaskSnapshotDomainModel snapshot);
        Task<IEnumerable<TaskSnapshotDomainModel>> GetSnapshotsByTaskIdAsync(Guid jobId);
        Task<TaskSnapshotDomainModel> GetLatestSnapshotAsync(Guid jobId);
    }
}
