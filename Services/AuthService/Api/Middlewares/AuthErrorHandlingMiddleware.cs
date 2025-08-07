using AuthService.App.Exceptions;
using TaskManagementServices.Shared;

namespace AuthService.Api.Middlewares
{
    internal class AuthErrorHandlingMiddleware : ErrorHandlingMiddlewareBase
    {
        public AuthErrorHandlingMiddleware(ILogger<ErrorHandlingMiddlewareBase> log, RequestDelegate next) : base(log, next)
        {
        }

        protected override async Task<bool> HandleException(Exception ex, HttpContext context)
        {
            switch (ex)
            {
                case UserAlreadyExistsException:
                    _log.LogDebug(ex, "User already exists exception handled");
                    context.Response.StatusCode = StatusCodes.Status409Conflict;
                    await context.Response.WriteAsync(ex.Message);
                    return true;
                case InvalidCredentialsException:
                    _log.LogDebug(ex, "Invalid credentials exception handled");
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync(ex.Message);
                    return true;
            }
            return false;
        }
    }
}