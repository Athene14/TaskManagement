using HealthCheck;
using Microsoft.OpenApi.Models;
using NotificationService.Api.Extentions;
using NotificationService.Api.Middleware;
using TaskManagementServices.Shared.NotificationService;
using Database.Dapper;
using Logging;

namespace NotificationService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var serviceConfig = new NotificationServiceConfiguration(builder.Configuration);

            builder.WebHost.ConfigureKestrel(serverOptions =>
            {
                serverOptions.ListenAnyIP(serviceConfig.GetPort());
            });

            // Add services
            builder.Services.AddControllers();
            builder.Logging.AddConfiguration(builder.Configuration);
            builder.Services.ConfigureDatabase(builder.Configuration);
            builder.Services.ConfigureNotificationServices();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.UseHealthCheck();
            builder.Logging.AddCustomConsoleFormat();

            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Notification Service", Version = "v1" });
            });

            var app = builder.Build();

            // Initialize database
            app.InitDataBase().Wait();

            app.MapHealthCheckEndpoints(serviceConfig.GetHealthCheckRelativePath());

            // Middleware pipeline
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Notification Service API v1");
                    c.DisplayRequestDuration();
                    c.EnableTryItOutByDefault();
                });
            }

            app.UseMiddleware<NotificationErrorHandlingMiddleware>();
            app.MapControllers();
            app.Run();
        }
    }
}
