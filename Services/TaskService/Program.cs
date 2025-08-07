using HealthCheck;
using Microsoft.OpenApi.Models;
using TaskManagementServices.Shared.TaskService;
using TaskService.Api.Extentions;
using TaskService.Api.Middleware;
using Database.Dapper;
using Logging;

namespace TaskService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var serviceConfig = new TaskServiceConfiguration(builder.Configuration);

            builder.WebHost.ConfigureKestrel(serverOptions =>
            {
                serverOptions.ListenAnyIP(serviceConfig.GetPort());
            });

            builder.Services.AddControllers();
            builder.Logging.AddConfiguration(builder.Configuration);
            builder.Services.ConfigureService(builder.Configuration);
            builder.Services.ConfigureDatabase(builder.Configuration);
            builder.Services.UseHealthCheck();
            builder.Logging.AddCustomConsoleFormat();


            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Task Service", Version = "v1" });
            });

            var app = builder.Build();

            app.InitDataBase().Wait();

            app.MapHealthCheckEndpoints(serviceConfig.GetHealthCheckRelativePath());

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Task Service API v1");
                    c.DisplayRequestDuration();
                    c.EnableTryItOutByDefault();
                });
            }

            app.UseMiddleware<TaskErrorHandlingMiddleware>();
            app.MapControllers();
            app.Run();
        }
    }
}
