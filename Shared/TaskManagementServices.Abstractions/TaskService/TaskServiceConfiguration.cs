using Microsoft.Extensions.Configuration;

namespace TaskManagementServices.Shared.TaskService
{
    public class TaskServiceConfiguration : ServiceConfigurationBase
    {
        public const string TaskServiceSectionStringName = "TaskService";
        private const string _healthCheckRelativePathStringName = "HealthCheckRelativePath";

        private readonly string _healthCheckRelativePath = "health-check";

        public TaskServiceConfiguration(IConfiguration config, string serviceSectionName = TaskServiceSectionStringName) : base(config, serviceSectionName)
        {
            var section = config.GetSection(TaskServiceSectionStringName);
            _healthCheckRelativePath = section.GetValue<string>(_healthCheckRelativePathStringName);
        }

        public string GetHealthCheckRelativePath()
        {
            return _healthCheckRelativePath;
        }
    }
}
