using Microsoft.AspNetCore.Mvc;
using TaskManagementServices.Shared.AuthService;
using TaskManagementServices.Shared.AuthService.DTO;

namespace AuthService.Api.Controllers
{
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
            => _authService = authService;


        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var resp = await _authService.RegisterAsync(request);
            return Ok(resp);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var resp = await _authService.LoginAsync(request);
            return Ok(resp);
        }

        [HttpGet("get-user/{userId:guid}")]
        public async Task<IActionResult> GetUserById(Guid userId)
        {
            var user = await _authService.GetUserAsync(userId);

            return Ok(user);
        }
    }
}
