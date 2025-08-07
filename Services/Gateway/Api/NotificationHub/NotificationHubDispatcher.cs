using Gateway.Domain.Model;
using Microsoft.AspNetCore.SignalR;

namespace Gateway.Api.NotificationHub
{
    public class NotificationHubDispatcher : INotificationHub
    {
        private readonly IHubContext<NotificationHub> _hubContext;

        public NotificationHubDispatcher(IHubContext<NotificationHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task OnNotificationCreated(Guid userId, SignalRNotificationModel model)
        {
            await _hubContext.Clients.User(userId.ToString())
                .SendAsync("OnNotificationCreated", model);
        }
    }
}
