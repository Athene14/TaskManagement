namespace AuthService.Infra.Security
{
    internal class PasswordHasher : IPasswordHasher
    {
        public string HashPassword(string password)
            => BCrypt.Net.BCrypt.EnhancedHashPassword(password);

        public bool VerifyPassword(string hash, string password)
            => string.IsNullOrEmpty(password)? false: BCrypt.Net.BCrypt.EnhancedVerify(password, hash);
    }
}
