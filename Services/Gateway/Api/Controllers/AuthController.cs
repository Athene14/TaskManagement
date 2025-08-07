using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;
using System.Security.Claims;
using TaskManagementServices.Shared.AuthService;
using TaskManagementServices.Shared.AuthService.DTO;

namespace Gateway.Api.Controllers
{
    [ApiController]
    [Route("api/auth")]
    [Produces("application/json")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IMemoryCache _cache;

        public AuthController(IAuthService authService, IMemoryCache cache)
        {
            _authService = authService;
            _cache = cache;
        }

        [HttpPost("register")]
        [SwaggerResponse((int)HttpStatusCode.OK, "RegisterRequest", typeof(LoginResponse))]
        public async Task<ActionResult<UserResponse>> Register([FromBody] RegisterRequest request)
        {
            var resp = await _authService.RegisterAsync(request);
            return Ok(resp);
        }



        [HttpPost("login")]
        [SwaggerResponse((int)HttpStatusCode.OK, "LoginRequest", typeof(LoginResponse))]
        public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
        {
            var result = await _authService.LoginAsync(request);
            return Ok(result);
        }


        [Authorize]
        [HttpGet("me")]
        [SwaggerResponse((int)HttpStatusCode.OK, "Get user by ID", typeof(UserResponse))]
        public async Task<IActionResult> GetMe()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var cacheKey = $"auth_user_{userId}";

            if (!_cache.TryGetValue(cacheKey, out UserResponse result))
            {
                result = await _authService.GetUserAsync(Guid.Parse(userId!));
                _cache.Set(cacheKey, result, TimeSpan.FromMinutes(3));
            }

            return Ok(result);
        }
    }
}
