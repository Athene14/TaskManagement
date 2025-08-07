using TaskManagementServices.Shared;

namespace NotificationService.Api.Middleware
{
    internal class NotificationErrorHandlingMiddleware : ErrorHandlingMiddlewareBase
    {
        public NotificationErrorHandlingMiddleware(ILogger<ErrorHandlingMiddlewareBase> log, RequestDelegate next) : base(log, next)
        {
        }

        protected override Task<bool> HandleException(Exception ex, HttpContext context)
        {
            return Task.FromResult(false);
        }
    }
}
