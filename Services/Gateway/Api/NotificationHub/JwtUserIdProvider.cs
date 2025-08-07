using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace Gateway.Api.NotificationHub
{
    public class CustomUserIdProvider : IUserIdProvider
    {
        public string GetUserId(HubConnectionContext connection)
        {
            // Извлекаем UserId из claims авторизованного пользователя
            return connection.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
    }
}
