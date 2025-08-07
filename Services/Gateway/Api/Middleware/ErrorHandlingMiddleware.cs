using Polly.CircuitBreaker;

namespace Gateway.Api.Middleware
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlingMiddleware> _log;
        public ErrorHandlingMiddleware(ILogger<ErrorHandlingMiddleware> log, RequestDelegate next)
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
            catch (HttpRequestException ex) when (ex.StatusCode.HasValue)
            {
                context.Response.StatusCode = (int)ex.StatusCode;
                await context.Response.WriteAsJsonAsync(new { error = ex.Message });
            }
            catch (BrokenCircuitException ex)
            {
                _log.LogWarning(ex, "Circuit breaker opened");
                context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
                await context.Response.WriteAsJsonAsync(new { error = "Service unavailable. Try again later." });
            }
            catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
            {
                _log.LogWarning(ex, "Request timeout");
                context.Response.StatusCode = StatusCodes.Status504GatewayTimeout;
                await context.Response.WriteAsJsonAsync(new { error = "Request timeout" });
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Unhandled exception");
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await context.Response.WriteAsJsonAsync(new { error = "Internal server error" });
            }
        }
    }
}
