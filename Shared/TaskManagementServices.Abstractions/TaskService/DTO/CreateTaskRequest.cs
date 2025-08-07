namespace TaskManagementServices.Shared.TaskService.DTO
{
    public class CreateTaskRequest
    {
        /// <summary>
        /// Заголовок задачи
        /// </summary>
        public required string Title { get; set; }

        /// <summary>
        /// Описание задачи
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Id назначенного пользователя
        /// </summary>
        public Guid? AssignedUserId { get; set; }
    }
}
