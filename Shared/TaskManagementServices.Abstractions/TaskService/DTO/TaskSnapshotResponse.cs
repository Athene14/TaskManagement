namespace TaskManagementServices.Shared.TaskService.DTO
{
    public class TaskSnapshotResponse
    {
        /// <summary>
        /// Id снапшота
        /// </summary>
        public Guid SnapshotId { get; set; }

        /// <summary>
        /// Id пользователя, который изменил задачу
        /// </summary>
        public Guid ChangedBy { get; set; }

        /// <summary>
        /// Заголовок после изменения
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Описание после изменения
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Время изменения
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Является ли задача активной после изменения
        /// </summary>
        public bool IsActive { get; set; } = false;

        /// <summary>
        /// Id назначенного пользователя после изменения
        /// </summary>
        public Guid? AssignedUserId { get; set; }
    }
}
