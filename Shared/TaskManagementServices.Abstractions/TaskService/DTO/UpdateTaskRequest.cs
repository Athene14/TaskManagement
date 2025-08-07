namespace TaskManagementServices.Shared.TaskService.DTO
{
    public class UpdateTaskRequest
    {
        /// <summary>
        /// Новый заголовок
        /// </summary>
        public string? Title { get; set; }

        /// <summary>
        /// Новое описание
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Id пользователя, на которого назначена задача
        /// </summary>
        public Guid? AssignedUserId { get; set; }
    }
}
