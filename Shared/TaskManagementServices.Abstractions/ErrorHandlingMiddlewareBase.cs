using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using TaskManagementServices.Shared.Exceptions;

namespace TaskManagementServices.Shared
{
    public abstract class ErrorHandlingMiddlewareBase
    {
        private readonly RequestDelegate _next;
        protected ILogger<ErrorHandlingMiddlewareBase> _log;

        public ErrorHandlingMiddlewareBase(ILogger<ErrorHandlingMiddlewareBase> log, RequestDelegate next)
        {
            _log = log;
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (InvalidArgumentException ex)
            {
                _log.LogDebug(ex, "Argument exception handled");
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsync(ex.Message);
            }
            catch (NotFoundException ex)
            {
                _log.LogDebug(ex, "Not found exception handled");
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                await context.Response.WriteAsync(ex.Message);
            }
            catch (Exception ex)
            {
                var isHandled = await HandleException(ex, context);
                if (!isHandled)
                {
                    _log.LogError(ex, "Exception is not handled!");
                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    await context.Response.WriteAsync("Internal server error");
                }
            }
        }

        /// <summary>
        /// Обрабатывает исключение, которое не описано в Invoke
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="context"></param>
        /// <returns>true, если исключение обработано, иначе - false</returns>
        protected abstract Task<bool> HandleException(Exception ex, HttpContext context);
    }
}
