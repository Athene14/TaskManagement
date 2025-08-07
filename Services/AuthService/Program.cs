using AuthService.Api.Extentions;
using AuthService.Api.Middlewares;
using HealthCheck;
using Microsoft.OpenApi.Models;
using TaskManagementServices.Shared.AuthService;
using Database.Dapper;
using Logging;

namespace AuthService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var serviceConfig = new AuthServiceConfiguration(builder.Configuration);

            builder.WebHost.ConfigureKestrel(serverOptions =>
            {
                serverOptions.ListenAnyIP(serviceConfig.GetPort());
            });

            // Add services
            builder.Services.AddControllers();
            builder.Logging.AddConfiguration(builder.Configuration);
            builder.Services.ConfigureDatabase(builder.Configuration);
            builder.Services.ConfigureAuthServices();
            builder.Services.UseHealthCheck();
            builder.Logging.AddCustomConsoleFormat();

            builder.Services.AddEndpointsApiExplorer();


            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Auth Service", Version = "v1" });
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
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Auth Service API v1");
                    c.DisplayRequestDuration();
                    c.EnableTryItOutByDefault();
                });
            }

            app.UseMiddleware<AuthErrorHandlingMiddleware>();
            app.MapControllers();
            app.Run();
        }
    }
}
