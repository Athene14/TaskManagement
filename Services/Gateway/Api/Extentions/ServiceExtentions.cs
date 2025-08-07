using Gateway.Api.Middleware;
using Gateway.Api.NotificationHub;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.SignalR;
using Microsoft.IdentityModel.Tokens;
using Polly;
using Polly.Extensions.Http;
using System.Net;
using TaskManagementServices.Shared.AuthService;
using TaskManagementServices.Shared.NotificationService;
using TaskManagementServices.Shared.Routers;
using TaskManagementServices.Shared.TaskService;

namespace Gateway.Api.Extentions
{
    public static class ServiceExtentions
    {
        public static IServiceCollection ConfigureGatewayRouters(this IServiceCollection services)
        {
            services.AddScoped<INotificationHub, NotificationHubDispatcher>();
            services.AddSingleton<IUserIdProvider, CustomUserIdProvider>();
            services.AddMemoryCache();
            services.AddHttpClient("TaskService", client =>
            {
                client.Timeout = TimeSpan.FromSeconds(15);
            })
            .AddHttpMessageHandler<HttpResponseMessageHandler>()
            .AddPolicyHandler(GetRetryPolicy())
            .AddPolicyHandler(GetCircuitBreakerPolicy());


            services.AddAuthServiceRouter();
            services.AddTasksServiceRouter();
            services.AddNotificationServiceRouter();

            return services;
        }

        public static void ConfigureJwt(this IServiceCollection services, IConfiguration config)
        {
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = config["Jwt:Issuer"],
                        ValidAudience = config["Jwt:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(
                            System.Text.Encoding.UTF8.GetBytes(config["Jwt:Key"]))
                    };
                });

            services.AddAuthentication();
        }

        public static IServiceCollection ConfigureServicesHealthCheck(this IServiceCollection collection, IConfiguration config)
        {
            var authConf = new AuthServiceConfiguration(config);
            var taskConf = new TaskServiceConfiguration(config);
            var notificationConf = new NotificationServiceConfiguration(config);

            collection.AddHealthChecksUI(setup =>
            {
                setup.AddHealthCheckEndpoint("Auth Service", GetHealthEndpointPath(authConf.GetHost(), authConf.GetPort(), authConf.GetHealthCheckRelativePath()));
                setup.AddHealthCheckEndpoint("Task Service", GetHealthEndpointPath(taskConf.GetHost(), taskConf.GetPort(), taskConf.GetHealthCheckRelativePath()));
                setup.AddHealthCheckEndpoint("Notification Service", GetHealthEndpointPath(notificationConf.GetHost(), notificationConf.GetPort(), notificationConf.GetHealthCheckRelativePath()));

                // Настройки
                setup.SetEvaluationTimeInSeconds(30);
                setup.SetApiMaxActiveRequests(3);
                setup.MaximumHistoryEntriesPerEndpoint(100);
            })
            .AddInMemoryStorage();

            string GetHealthEndpointPath(string host, int port, string relativePath)
            {
                return $"http://{host}:{port}/{relativePath}";
            }

            return collection;
        }

        public static WebApplication UseGatewayHealthCheckUI(this WebApplication app, string relativePath)
        {
            app.UseHealthChecksUI(options =>
            {
                options.UIPath = $"/{relativePath}-ui";
                options.ApiPath = $"/{relativePath}";
                options.UseRelativeApiPath = true;
                options.UseRelativeResourcesPath = true;
            });
            return app;
        }

        private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(msg => msg.StatusCode == HttpStatusCode.TooManyRequests)
                .Or<TimeoutException>()
                .WaitAndRetryAsync(3, retryAttempt =>
                    TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
        }

        private static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .Or<TimeoutException>()
                .CircuitBreakerAsync(
                    handledEventsAllowedBeforeBreaking: 5,
                    durationOfBreak: TimeSpan.FromSeconds(30)
                );
        }
    }
}
