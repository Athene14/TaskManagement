using Gateway.Api.NotificationHub;
using Gateway.Domain.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Memory;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;
using System.Security.Claims;
using TaskManagementServices.Shared.NotificationService;
using TaskManagementServices.Shared.NotificationService.DTO;

namespace Gateway.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/notifications")]
    [Produces("application/json")]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _service;
        private readonly INotificationHub _notificationHub;
        private readonly IMemoryCache _cache;

        public NotificationsController(INotificationService service, INotificationHub notificationHub, IMemoryCache cache)
        {
            _cache = cache;
            _service = service;
            _notificationHub = notificationHub;
        }

        // Вспомогательный метод для получения текущего UserId из JWT
        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out Guid userId))
            {
                throw new UnauthorizedAccessException("Invalid user identifier");
            }
            return userId;
        }

        [HttpGet("{userId:guid}")]
        [SwaggerResponse((int)HttpStatusCode.OK, "Notifications list", typeof(List<NotificationResponse>))]
        public async Task<ActionResult<IEnumerable<NotificationResponse>>> GetUserNotifications(Guid userId, [FromQuery] bool unreadOnly = false)
        {
            var cacheKey = $"notifications_{userId}_{unreadOnly}";

            if (_cache.TryGetValue(cacheKey, out List<NotificationResponse>? cachedNotifications))
            {
                return Ok(cachedNotifications);
            }

            var notifications = await _service.GetUserNotificationsAsync(userId, unreadOnly);

            _cache.Set(cacheKey, notifications, TimeSpan.FromMinutes(1));

            return Ok(notifications);
        }

        [HttpPost]
        [SwaggerResponse((int)HttpStatusCode.Created, "Created notification id", typeof(CreateNotificationResponse))]
        public async Task<ActionResult<CreateNotificationResponse>> CreateNotification(CreateNotificationRequest request)
        {
            var createdNotification = await _service.CreateNotificationAsync(request);

            foreach (var recipientId in request.RecipientIds)
            {
                // удаляем оба значения
                _cache.Remove($"notifications_{recipientId}_true");
                _cache.Remove($"notifications_{recipientId}_false");
            }

            var notificationModel = new SignalRNotificationModel
            {
                NotificationId = createdNotification.CreatedNotificationId,
                Message = request.Message
            };

            var sendNotificationTasks = request.RecipientIds.Distinct().Select(t =>
                        _notificationHub.OnNotificationCreated(t, notificationModel));
            
            await Task.WhenAll(sendNotificationTasks);

            return CreatedAtAction(nameof(CreateNotification), createdNotification);
        }

        [HttpPut("{notificationId:guid}/mark-as-read")]
        [SwaggerResponse((int)HttpStatusCode.OK, "Notification marked")]
        public async Task<IActionResult> MarkAsRead(Guid notificationId)
        {
            var recipientId = GetCurrentUserId();
            await _service.MarkAsReadAsync(notificationId, recipientId);

            // Удаляем оба варианта кэша
            _cache.Remove($"notifications_{recipientId}_true");
            _cache.Remove($"notifications_{recipientId}_false");

            return Ok();
        }
    }
}
