using NotificationService.Domain.Abstractions;
using NotificationService.Infra.Data.Repositories;
using TaskManagementServices.Shared.NotificationService;
using Database.Dapper;

namespace NotificationService.Api.Extentions
{
    internal static class ServiceExtentions
    {
        public static void ConfigureDatabase(this IServiceCollection services, IConfiguration config)
        {
            var serviceConfiguration = new NotificationServiceConfiguration(config);
            services.AddPgSqlDatabase(serviceConfiguration.GetServiceSectionName());
            services.AddScoped<INotificationRecipientRepository, NotificationRecipientRepository>();
            services.AddScoped<INotificationRepository, NotificationRepository>();
        }

        public static void ConfigureNotificationServices(this IServiceCollection services)
        {
            services.AddScoped<INotificationService, NotificationService.App.Service.NotificationService>();
        }
    }
}
