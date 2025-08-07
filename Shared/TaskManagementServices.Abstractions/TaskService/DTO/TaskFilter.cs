namespace TaskManagementServices.Shared.TaskService.DTO
{
    public class TaskFilter
    {
        /// <summary>
        /// Заголовок задачи
        /// </summary>
        public string? Title { get; set; }

        /// <summary>
        /// Назначенный пользователь
        /// </summary>
        public Guid? AssignedUserId { get; set; }

        /// <summary>
        /// Создано после определённого времени (Timestamp secounds)
        /// </summary>
        public long? CreatedFromTimestamp { get; set; }

        /// <summary>
        /// Создано до определённого времени (Timestamp secounds)
        /// </summary>
        public long? CreatedToTimestamp { get; set; }

        /// <summary>
        /// Только активные
        /// </summary>
        public bool? OnlyActive { get; set; }

        /// <summary>
        /// Id того, кто создал задачу
        /// </summary>
        public Guid? CreatedBy { get; set; }
    }
}
