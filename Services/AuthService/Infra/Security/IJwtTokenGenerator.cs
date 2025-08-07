using AuthService.Domain.Models;

namespace AuthService.Infra.Security
{
    public interface IJwtTokenGenerator
    {
        string GenerateToken(UserDomain user);
    }
}
