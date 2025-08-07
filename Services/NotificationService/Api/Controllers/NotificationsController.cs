using Microsoft.AspNetCore.Mvc;
using TaskManagementServices.Shared.NotificationService;
using TaskManagementServices.Shared.NotificationService.DTO;

namespace NotificationService.Api.Controllers
{
    [Route("")]
    [ApiController]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationsController(
            INotificationService notificationService,
            ILogger<NotificationsController> logger)
        {
            _notificationService = notificationService;
        }

        [HttpGet("{userId:guid}")]
        public async Task<IActionResult> GetNotifications(Guid userId, [FromQuery] bool unreadOnly = false)
        {
            var notifications = await _notificationService.GetUserNotificationsAsync(userId, unreadOnly);
            return Ok(notifications);
        }

        [HttpPost]
        public async Task<IActionResult> CreateNotification([FromBody] CreateNotificationRequest request)
        {
            var createdNotificationId = await _notificationService.CreateNotificationAsync(request);
            return Ok(createdNotificationId);

        }

        [HttpPut("{notificationId:guid}/mark-as-read")]
        public async Task<IActionResult> MarkAsRead(Guid notificationId, [FromQuery] Guid recipientId)
        {

            await _notificationService.MarkAsReadAsync(notificationId, recipientId);
            return NoContent();

        }

    }
}
