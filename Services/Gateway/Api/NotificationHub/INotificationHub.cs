using Gateway.Domain.Model;

namespace Gateway.Api.NotificationHub
{
    public interface INotificationHub
    {
        Task OnNotificationCreated(Guid userId, SignalRNotificationModel model);
    }
}
