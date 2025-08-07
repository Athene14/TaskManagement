using AuthService.Domain.Models;

namespace AuthService.Domain.Abstractions
{
    public interface IUserRepository : IDisposable
    {
        Task<Guid> CreateAsync(UserDomain user);
        Task<UserDomain> GetByIdAsync(Guid id);
        Task<(bool success, UserDomain user)> TryGetByEmailAsync(string email);
        Task<(bool success, UserDomain user)> TryGetByName(string name);
    }
}
