using Microsoft.Extensions.Logging;
using TaskManagementServices.Shared.NotificationService;
using TaskManagementServices.Shared.NotificationService.DTO;

namespace TaskManagementServices.Shared.Routers
{
    internal class NotificationServiceRouter : INotificationService
    {
        private readonly NotificationServiceConfiguration _config;
        private readonly HttpClient _httpClient;
        private readonly ILogger<NotificationServiceRouter> _logger;

        public NotificationServiceRouter(
            IHttpClientFactory httpClientFactory,
            NotificationServiceConfiguration configuration,
            ILogger<NotificationServiceRouter> logger)
        {
            _config = configuration;
            _logger = logger;
            _httpClient = httpClientFactory.CreateClient();
        }

        private string BaseUrl => $"http://{_config.GetHost()}:{_config.GetPort()}";

        public async Task<CreateNotificationResponse> CreateNotificationAsync(CreateNotificationRequest request)
        {
            var url = BaseUrl;
            _logger.LogDebug("Creating notification for {RecipientIds}", string.Join(',', request.RecipientIds));

            var response = await _httpClient.PostAndReadResponseAsync<CreateNotificationResponse>(url, request);

            _logger.LogInformation("Notification created | ID: {NotificationId}", response.CreatedNotificationId);

            return response;
        }

        public async Task<IEnumerable<NotificationResponse>> GetUserNotificationsAsync(Guid userId, bool unreadOnly)
        {
            var url = $"{BaseUrl}/{userId}?unreadOnly={unreadOnly}";
            _logger.LogDebug("Fetching notifications for user {UserId} | Unread only: {UnreadOnly}", userId, unreadOnly);

            var response = await _httpClient.GetAndReadResponseAsync<List<NotificationResponse>>(url);

            _logger.LogDebug("Retrieved {Count} notifications for user {UserId}", response.Count, userId);

            return response;
        }

        public async Task MarkAsReadAsync(Guid notificationId, Guid recipientId)
        {
            var url = $"{BaseUrl}/{notificationId}/mark-as-read?recipientId={recipientId}";
            _logger.LogDebug("Marking notification {NotificationId} as read for user {RecipientId}", notificationId, recipientId);

            await _httpClient.PutAndReadResponseAsync(url, null);

            _logger.LogInformation("Notification {NotificationId} marked as read for user {RecipientId}", notificationId, recipientId);
        }
    }
}
