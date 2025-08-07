using TaskManagementServices.Shared.TaskService;
using TaskService.Domain.Abstractions;
using TaskService.Infra.Data.Repositories;
using Database.Dapper;

namespace TaskService.Api.Extentions
{
    internal static class ServiceExtensions
    {
        public static IServiceCollection ConfigureDatabase(this IServiceCollection services, IConfiguration config)
        {
            var serviceConfiguration = new TaskServiceConfiguration(config);
            services.AddPgSqlDatabase(serviceConfiguration.GetServiceSectionName());
            services.AddScoped<ITaskHistoryRepository, TaskHistoryRepository>();
            services.AddScoped<ITaskRepository, TaskRepository>();
            return services;
        }

        public static IServiceCollection ConfigureService(this IServiceCollection services, IConfiguration config)
        {
            services.AddSingleton<TaskServiceConfiguration>();
            services.AddScoped<ITaskService, TaskService.App.Service.TaskService>();

            return services;
        }
    }
}
