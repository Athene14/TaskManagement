using NotificationService.Domain.Models;

namespace NotificationService.Domain.Abstractions
{
    public interface INotificationRecipientRepository : IDisposable
    {
        Task AddRecipientAsync(NotificationRecipientDomainModel recipient);
        Task<IEnumerable<NotificationRecipientDomainModel>> GetByUserIdAsync(Guid userId, bool unreadOnly);
        Task<IEnumerable<NotificationRecipientDomainModel>> GetRecipientsForNotificationAsync(Guid notificationId);
        Task MarkAsReadAsync(Guid recipientId, Guid notificationId);
        Task AddRecipientsAsync(IEnumerable<NotificationRecipientDomainModel> recipients);
        Task<NotificationRecipientDomainModel> GetByRecipientAndNotificationAsync(Guid recipientId, Guid notificationId);
    }
}
