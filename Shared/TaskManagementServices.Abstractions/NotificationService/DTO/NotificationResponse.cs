namespace TaskManagementServices.Shared.NotificationService.DTO
{
    public class NotificationResponse
    {
        /// <summary>
        /// Id уведомления
        /// </summary>
        public required Guid Id { get; set; }

        /// <summary>
        /// Id получателя
        /// </summary>
        public required Guid UserId { get; set; }

        /// <summary>
        /// Время создания
        /// </summary>
        public required DateTime Created { get; set; }

        /// <summary>
        /// Сообщение
        /// </summary>
        public required string Message { get; set; }

        /// <summary>
        /// Было ли прочитано
        /// </summary>
        public required bool IsRead { get; set; }
    }
}
