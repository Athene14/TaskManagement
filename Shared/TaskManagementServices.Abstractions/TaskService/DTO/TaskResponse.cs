namespace TaskManagementServices.Shared.TaskService.DTO
{
    public class TaskResponse
    {
        /// <summary>
        /// Id созданной задачи
        /// </summary>
        public required Guid Id { get; set; }

        /// <summary>
        /// Id того, кто задачу создал
        /// </summary>
        public required Guid CreatedBy { get; set; }

        /// <summary>
        /// Заголовок задачи
        /// </summary>
        public required string Title { get; set; }

        /// <summary>
        /// Описание задачи
        /// </summary>
        public required string Description { get; set; }

        /// <summary>
        /// Время создания задачи
        /// </summary>
        public required DateTime CreatedAt { get; set; }

        /// <summary>
        /// Время обновления задачи
        /// </summary>
        public required DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Является ли задача активной
        /// </summary>
        public required bool IsActive { get; set; } = false;

        /// <summary>
        /// Id назначенного пользователя
        /// </summary>
        public Guid? AssignedUserId { get; set; }
    }
}
