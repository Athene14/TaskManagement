using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthCheck
{
    public static class HealthCheckStartupExtention
    {
        public static IServiceCollection UseHealthCheck(this IServiceCollection collection)
        {
            collection.AddHealthChecks()
                .AddCheck<DatabaseHealthCheck.PgSqlDatabaseHealthCheck>(
                    "db_check",
                    failureStatus: HealthStatus.Unhealthy,
                    tags: new[] { "database" })
                .AddCheck("liveness_check", () =>
                    HealthCheckResult.Healthy("Service is alive"),
                    tags: new[] { "live" });

            return collection;
        }

        public static IServiceCollection AddHealthCheckUI(this IServiceCollection collection, string serviceName, string healthUrl)
        {
            collection.AddHealthChecksUI(options =>
            {
                options.SetEvaluationTimeInSeconds(60);
                options.MaximumHistoryEntriesPerEndpoint(50);
                options.AddHealthCheckEndpoint(serviceName, healthUrl);
            })
            .AddInMemoryStorage();
            return collection;
        }

        public static WebApplication MapHealthCheckEndpoints(this WebApplication app, string relativePath)
        {
            app.MapHealthChecks(relativePath, new HealthCheckOptions
            {
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
                ResultStatusCodes =
                    {
                        [HealthStatus.Healthy] = StatusCodes.Status200OK,
                        [HealthStatus.Degraded] = StatusCodes.Status200OK,
                        [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
                    },
                Predicate = _ => true,
                AllowCachingResponses = false
            });

            return app;
        }

        public static WebApplication UseHealthUI(this WebApplication app)
        {
            app.UseHealthChecksUI(options =>
            {
                options.UseRelativeResourcesPath = true;
                options.UseRelativeApiPath = true;
                options.UseRelativeWebhookPath = true;
                options.UIPath = "/health-ui";
            });
            return app;
        }
    }
}
