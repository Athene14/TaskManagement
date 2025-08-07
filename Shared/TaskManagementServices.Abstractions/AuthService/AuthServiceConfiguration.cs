using Microsoft.Extensions.Configuration;

namespace TaskManagementServices.Shared.AuthService
{
    public class AuthServiceConfiguration : ServiceConfigurationBase
    {
        public const string AuthServiceSectionStringName = "AuthService";
        private const string _healthCheckRelativePathStringName = "HealthCheckRelativePath";

        private readonly string _healthCheckRelativePath = "health-check";

        public AuthServiceConfiguration(IConfiguration config) : base(config, AuthServiceSectionStringName)
        {
            var section = config.GetSection(AuthServiceSectionStringName);
            _healthCheckRelativePath = section.GetValue<string>(_healthCheckRelativePathStringName);
        }

        public string GetHealthCheckRelativePath()
        {
            return _healthCheckRelativePath;
        }
    }
}
