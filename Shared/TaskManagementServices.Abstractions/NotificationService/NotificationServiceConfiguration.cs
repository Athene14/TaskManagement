using Microsoft.Extensions.Configuration;

namespace TaskManagementServices.Shared.NotificationService
{
    public class NotificationServiceConfiguration : ServiceConfigurationBase
    {
        public const string NotificationServiceSectionStringName = "NotificationService";
        private const string _healthCheckRelativePathStringName = "HealthCheckRelativePath";

        private readonly string _healthCheckRelativePath = "health-check";

        public NotificationServiceConfiguration(IConfiguration config, string serviceSectionName = NotificationServiceSectionStringName) : base(config, serviceSectionName)
        {
            var section = config.GetSection(NotificationServiceSectionStringName);
            _healthCheckRelativePath = section.GetValue<string>(_healthCheckRelativePathStringName);
        }

        public string GetHealthCheckRelativePath()
        {
            return _healthCheckRelativePath;
        }
    }
}
