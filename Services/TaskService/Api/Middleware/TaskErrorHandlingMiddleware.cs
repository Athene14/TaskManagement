using TaskManagementServices.Shared;
using TaskService.App.Exceptions;

namespace TaskService.Api.Middleware
{
    internal class TaskErrorHandlingMiddleware : ErrorHandlingMiddlewareBase
    {
        public TaskErrorHandlingMiddleware(ILogger<ErrorHandlingMiddlewareBase> log, RequestDelegate next) : base(log, next)
        {
        }

        protected override async Task<bool> HandleException(Exception ex, HttpContext context)
        {
            switch (ex)
            {
                case InactiveUpdateTaskException:
                    _log.LogDebug(ex, "Can't update not active task");
                    context.Response.StatusCode = StatusCodes.Status409Conflict;
                    await context.Response.WriteAsync(ex.Message);
                    return true;
            }
            return false;
        }
    }
}
