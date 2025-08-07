namespace AuthService.Domain.Models
{
    public class UserDomain
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string FullName { get; set; }
        public long CreatedAt { get; set; }
    }
}
