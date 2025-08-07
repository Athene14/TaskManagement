using Microsoft.Extensions.Configuration;

namespace Database.Dapper
{
    public class DatabaseConfiguration : IDatabaseConfiguration
    {
        private const string _sectionName = "DatabaseSettings";
        private const string _connectionStringName = "ConnectionString";
        private const string _migrateStringName = "Migrate";

        private readonly string _connectionString;
        private readonly bool _migrate;

        public DatabaseConfiguration(IConfiguration config, string serviceSectionName)
        {
            var serviceSettings = config.GetSection(serviceSectionName);
            var section = serviceSettings.GetSection(_sectionName);
            _connectionString = section.GetValue<string>(_connectionStringName);
            _migrate = section.GetValue<bool>(_migrateStringName);
        }

        public string GetConnectionString()
        {
            return _connectionString;
        }

        public bool Migrate()
        {
            return _migrate;
        }
    }
}
