using Gateway.Api.Extentions;
using Gateway.Api.Middleware;
using Gateway.Api.NotificationHub;
using Logging;
using Microsoft.OpenApi.Models;
using TaskManagementServices.Shared.Gateway;

namespace Gateway
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var serviceConfig = new GatewayConfiguration(builder.Configuration);

            builder.WebHost.ConfigureKestrel(serverOptions =>
            {
                serverOptions.ListenAnyIP(serviceConfig.GetPort());
            });

            builder.Logging.AddConfiguration(builder.Configuration);

            // Add services
            builder.Services.ConfigureGatewayRouters();
            builder.Services.AddControllers();
            builder.Logging.AddCustomConsoleFormat();
            builder.Services.AddSignalR(options =>
            {
                options.EnableDetailedErrors = true;
            });

            builder.Services.ConfigureJwt(builder.Configuration);

            builder.Services.ConfigureServicesHealthCheck(builder.Configuration);

            // Configure Swagger
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Task Management", Version = "v1" });
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header",
                    Scheme = "Bearer",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Name = "Authorization"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            },
                            Scheme = "oauth2",
                            Name = "Bearer",
                            In = ParameterLocation.Header
                        },

                        Array.Empty<string>()
                    }
                });
            });

            var app = builder.Build();
            app.UseGatewayHealthCheckUI(serviceConfig.GetHealthCheckRelativePath());
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Task Management API v1");
                c.DisplayRequestDuration();
                c.EnableTryItOutByDefault();
            });

            app.UseRouting();
            app.UseMiddleware<ErrorHandlingMiddleware>();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseWebSockets();
            app.UseEndpoints(x =>
            {
                x.MapControllers();
                x.MapHub<NotificationHub>("/notificationHub");
            });
            app.Run();
        }
    }
}
