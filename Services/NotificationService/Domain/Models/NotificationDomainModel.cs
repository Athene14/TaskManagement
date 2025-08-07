namespace NotificationService.Domain.Models
{
    public class NotificationDomainModel
    {
        public Guid Id { get; set; }
        public long CreatedTimestamp { get; set; }
        public required string Message { get; set; }
    }
}
