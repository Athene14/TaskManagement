namespace TaskService.Domain.Models
{
    public class TaskSnapshotDomainModel
    {
        public Guid SnapshotId { get; set; }
        public Guid TaskId { get; set; }
        public Guid ChangedBy { get; set; }
        public long ChangeTime { get; set; }

        public string Title { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public Guid? AssignedUserId { get; set; }
    }
}
