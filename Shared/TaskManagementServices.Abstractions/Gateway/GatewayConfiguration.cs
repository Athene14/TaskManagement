using Microsoft.Extensions.Configuration;

namespace TaskManagementServices.Shared.Gateway
{
    public class GatewayConfiguration : ServiceConfigurationBase
    {
        public const string GatewaySectionStringName = "Gateway";
        private const string _healthCheckRelativePathStringName = "HealthCheckRelativePath";

        private readonly string _healthCheckRelativePath = "health-check";

        public GatewayConfiguration(IConfiguration config, string serviceSectionName = GatewaySectionStringName) : base(config, serviceSectionName)
        {
            var section = config.GetSection(GatewaySectionStringName);
            _healthCheckRelativePath = section.GetValue<string>(_healthCheckRelativePathStringName);
        }

        public string GetHealthCheckRelativePath()
        {
            return _healthCheckRelativePath;
        }
    }
}
