using TaskManagementServices.Shared.AuthService.DTO;

namespace TaskManagementServices.Shared.AuthService
{
    public interface IAuthService
    {
        Task<UserResponse> RegisterAsync(RegisterRequest request);
        Task<LoginResponse> LoginAsync(LoginRequest request);
        Task<UserResponse> GetUserAsync(Guid userId);
    }
}
