namespace TaskManagementServices.Shared.NotificationService.DTO
{
    public class CreateNotificationRequest
    {
        /// <summary>
        /// Сообщение
        /// </summary>
        public required string Message { get; set; }

        /// <summary>
        /// Список получателей
        /// </summary>
        public required List<Guid> RecipientIds { get; set; }
    }
}
