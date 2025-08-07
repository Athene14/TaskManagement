using Gateway.Domain.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using TaskManagementServices.Shared.NotificationService.DTO;
using TaskManagementServices.Shared.NotificationService;

namespace Gateway.Api.NotificationHub
{
    [Authorize]
    public class NotificationHub : Hub
    {
        private readonly ILogger<NotificationHub> _logger;
        private readonly INotificationService _notificationService;

        public NotificationHub(
            ILogger<NotificationHub> logger,
            INotificationService notificationService)
        {
            _logger = logger;
            _notificationService = notificationService;
        }

        public async Task SendNotification(string message, List<Guid> recipientIds)
        {
            _logger.LogInformation("Recieved SendNotification request");

            var senderId = GetCurrentUserId();

            var request = new CreateNotificationRequest
            {
                Message = message,
                RecipientIds = recipientIds
            };

            var response = await _notificationService.CreateNotificationAsync(request);

            var notificationModel = new SignalRNotificationModel
            {
                NotificationId = response.CreatedNotificationId,
                Message = message
            };

            await SendToRecipients(recipientIds, notificationModel);
        }

        private async Task SendToRecipients(List<Guid> recipientIds, SignalRNotificationModel model)
        {
            var recipientTasks = recipientIds.Select(userId =>
                Clients.User(userId.ToString()).SendAsync("OnNotificationCreated", model)
            );

            await Task.WhenAll(recipientTasks);
        }

        private Guid GetCurrentUserId()
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var id))
            {
                throw new UnauthorizedAccessException("Invalid user identity");
            }
            return id;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation($"User {userId} connected. Connection ID: {Context.ConnectionId}");

            await Groups.AddToGroupAsync(Context.ConnectionId, userId.ToString());

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = GetCurrentUserId();

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, userId.ToString());

            if (exception != null)
            {
                _logger.LogError(exception, $"User {userId} disconnected unexpectedly");
            }
            else
            {
                _logger.LogDebug($"User {userId} disconnected");
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}
