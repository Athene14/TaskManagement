namespace NotificationService.Domain.Models
{
    public class NotificationRecipientDomainModel
    {
        public Guid Id { get; set; }
        public Guid NotificationId { get; set; }
        public Guid RecipientId { get; set; }
        public bool IsRead { get; set; }
    }
}
