using Microsoft.Extensions.DependencyInjection;
using TaskManagementServices.Shared.AuthService;
using TaskManagementServices.Shared.NotificationService;
using TaskManagementServices.Shared.TaskService;

namespace TaskManagementServices.Shared.Routers
{
    public static class RoutersStartupExtentions
    {
        public static IServiceCollection AddAuthServiceRouter(this IServiceCollection collection)
        {
            collection.AddSingleton<AuthServiceConfiguration>();
            collection.AddScoped<IAuthService, AuthServiceRouter>();

            return collection;
        }

        public static IServiceCollection AddTasksServiceRouter(this IServiceCollection collection)
        {
            collection.AddSingleton<TaskServiceConfiguration>();
            collection.AddScoped<ITaskService, TaskServiceRouter>();

            return collection;
        }

        public static IServiceCollection AddNotificationServiceRouter(this IServiceCollection collection)
        {
            collection.AddSingleton<NotificationServiceConfiguration>();
            collection.AddScoped<INotificationService, NotificationServiceRouter>();

            return collection;
        }
    }
}
