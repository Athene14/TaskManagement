using Microsoft.Extensions.Logging;
using TaskManagementServices.Shared.AuthService;
using TaskManagementServices.Shared.AuthService.DTO;

namespace TaskManagementServices.Shared.Routers
{
    internal class AuthServiceRouter : IAuthService
    {
        private readonly AuthServiceConfiguration _config;
        private readonly HttpClient _httpClient;
        private readonly ILogger<AuthServiceRouter> _log;

        public AuthServiceRouter(
            IHttpClientFactory httpClientFactory,
            ILogger<AuthServiceRouter> log,
            AuthServiceConfiguration configuration)
        {
            _config = configuration;
            _log = log;
            _httpClient = httpClientFactory.CreateClient();
        }

        private string BaseUrl => $"http://{_config.GetHost()}:{_config.GetPort()}";

        public async Task<UserResponse> GetUserAsync(Guid userId)
        {
            var url = BaseUrl + $"/get-user/{userId}";
            _log.LogDebug("Requesting user {UserId} from auth service", userId);

            var response = await _httpClient.GetAndReadResponseAsync<UserResponse>(url);
            _log.LogDebug("Received user data for {UserId}", userId);

            return response;
        }

        public async Task<UserResponse> RegisterAsync(RegisterRequest request)
        {
            var url = BaseUrl + "/register";
            _log.LogDebug("Registering new user with email: {Email}", request.Email);

            var response = await _httpClient.PostAndReadResponseAsync<UserResponse>(url, request);
            _log.LogInformation("New user registered: {Email} | User ID: {UserId}",
                                request.Email, response.UserId);

            return response;
        }

        public async Task<LoginResponse> LoginAsync(LoginRequest request)
        {
            var url = BaseUrl + "/login";
            _log.LogDebug("Login attempt for: {Email}", request.Email);

            var response = await _httpClient.PostAndReadResponseAsync<LoginResponse>(url, request);
            _log.LogInformation("Successful login for: {Email}", request.Email);

            return response;
        }
    }
}
