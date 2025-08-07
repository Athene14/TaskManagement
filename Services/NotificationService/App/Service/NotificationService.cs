using NotificationService.Domain.Abstractions;
using NotificationService.Domain.Models;
using TaskManagementServices.Shared.Exceptions;
using TaskManagementServices.Shared.NotificationService;
using TaskManagementServices.Shared.NotificationService.DTO;

namespace NotificationService.App.Service
{
    internal class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepo;
        private readonly INotificationRecipientRepository _recipientRepo;

        public NotificationService(
            INotificationRepository notificationRepo,
            INotificationRecipientRepository recipientRepo)
        {
            _notificationRepo = notificationRepo;
            _recipientRepo = recipientRepo;
        }

        public async Task<IEnumerable<NotificationResponse>> GetUserNotificationsAsync(Guid userId, bool unreadOnly)
        {
            var recipients = await _recipientRepo.GetByUserIdAsync(userId, unreadOnly);
            var notificationIds = recipients.Select(r => r.NotificationId).Distinct().ToArray();

            if (!notificationIds.Any())
            {
                return Enumerable.Empty<NotificationResponse>();
            }

            var notifications = await _notificationRepo.GetManyByIds(notificationIds);
            var notificationsDict = notifications.ToDictionary(n => n.Id);

            return recipients.Select(r => new NotificationResponse
            {
                Id = r.NotificationId,
                Created = DateTimeOffset.FromUnixTimeSeconds(notificationsDict[r.NotificationId].CreatedTimestamp).DateTime,
                Message = notificationsDict[r.NotificationId].Message,
                IsRead = r.IsRead,
                UserId = r.RecipientId
            });
        }

        public async Task<CreateNotificationResponse> CreateNotificationAsync(CreateNotificationRequest request)
        {
            if (request == null)
                throw new InvalidArgumentException(nameof(request));

            if (string.IsNullOrWhiteSpace(request.Message))
                throw new InvalidArgumentException("Notification message cannot be empty");

            if (request.RecipientIds == null || !request.RecipientIds.Any())
                throw new InvalidArgumentException("Recipient list cannot be empty");

            var notification = new NotificationDomainModel
            {
                Id = Guid.NewGuid(),
                CreatedTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                Message = request.Message
            };

            await _notificationRepo.CreateAsync(notification);

            var recipients = request.RecipientIds.Distinct().Select(recipientId => new NotificationRecipientDomainModel
            {
                Id = Guid.NewGuid(),
                NotificationId = notification.Id,
                RecipientId = recipientId,
                IsRead = false
            }).ToList();

            await _recipientRepo.AddRecipientsAsync(recipients);

            return new CreateNotificationResponse() { CreatedNotificationId = notification.Id };
        }

        public async Task MarkAsReadAsync(Guid notificationId, Guid recipientId)
        {
            if (notificationId == Guid.Empty)
                throw new InvalidArgumentException($"Notification id is required");
            if (recipientId == Guid.Empty)
                throw new InvalidArgumentException($"User id is required");

            await _recipientRepo.MarkAsReadAsync(recipientId, notificationId);
        }
    }
}
