namespace Gateway.Domain.Model
{
    public class SignalRNotificationModel
    {
        /// <summary>
        /// Id уведомления
        /// </summary>
        public Guid NotificationId { get; set; }

        /// <summary>
        /// Сообщение
        /// </summary>
        public string Message { get; set; }
    }
}
