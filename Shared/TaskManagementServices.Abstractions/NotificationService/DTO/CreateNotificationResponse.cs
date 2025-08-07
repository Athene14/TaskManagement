namespace TaskManagementServices.Shared.NotificationService.DTO
{
    public class CreateNotificationResponse
    {
        /// <summary>
        /// Id созданного уведомления
        /// </summary>
        public required Guid CreatedNotificationId { get; set; }
    }
}
