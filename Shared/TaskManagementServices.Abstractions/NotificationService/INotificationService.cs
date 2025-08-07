using TaskManagementServices.Shared.NotificationService.DTO;

namespace TaskManagementServices.Shared.NotificationService
{
    public interface INotificationService
    {
        Task<IEnumerable<NotificationResponse>> GetUserNotificationsAsync(Guid userId, bool unreadOnly);
        Task<CreateNotificationResponse> CreateNotificationAsync(CreateNotificationRequest request);
        Task MarkAsReadAsync(Guid notificationId, Guid recipientId);
    }
}
