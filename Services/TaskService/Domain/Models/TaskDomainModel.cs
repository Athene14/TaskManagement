namespace TaskService.Domain.Models
{
    public class TaskDomainModel
    {
        public Guid TaskId { get; set; }
        public Guid CreatedBy { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public long CreatedAt { get; set; }
        public long UpdatedAt { get; set; }
        public bool IsActive { get; set; } = true;
        public Guid? AssignedUserId { get; set; }
    }
}
