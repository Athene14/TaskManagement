using Microsoft.Extensions.Configuration;

namespace TaskManagementServices.Shared
{
    public abstract class ServiceConfigurationBase
    {
        private const string _portStringName = "Port";
        private const string _hostStringName = "Host";

        private readonly int _port;
        private readonly string _host;
        private readonly string _serviceSectionName;

        public ServiceConfigurationBase(IConfiguration config, string serviceSectionName)
        {
            _serviceSectionName = serviceSectionName;
            var section = config.GetSection(serviceSectionName);
            _port = section.GetValue<int>(_portStringName);
            _host = section.GetValue<string>(_hostStringName);
        }

        public string GetServiceSectionName() => _serviceSectionName;

        public int GetPort() => _port;

        public string GetHost() => _host;
    }
}
