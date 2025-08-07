using Dapper;
using Database;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using System.Data;

namespace HealthCheck.DatabaseHealthCheck
{
    public class PgSqlDatabaseHealthCheck : IHealthCheck
    {
        private readonly IDatabaseConnectionFactory _connectionFactory;
        private readonly ILogger<PgSqlDatabaseHealthCheck> _logger;

        public PgSqlDatabaseHealthCheck(IDatabaseConnectionFactory connectionFactory, ILogger<PgSqlDatabaseHealthCheck> logger)
        {
            _logger = logger;
            _connectionFactory = connectionFactory;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                using var conn = await _connectionFactory.GetConnection();

                // 2. Проверка основных операций
                var canRead = await CheckReadAccess(conn);

                // 3. Сбор метрик
                var metrics = new Dictionary<string, object>
                {
                    ["active_connections"] = await GetActiveConnections(conn),
                    ["db_version"] = await GetPostgresVersion(conn)
                };

                return canRead
                    ? HealthCheckResult.Healthy("Database fully operational", metrics)
                    : HealthCheckResult.Degraded("Database partial failure", data: metrics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database health check failed");
                return HealthCheckResult.Unhealthy("Database connection error", ex);
            }
        }

        private async Task<bool> CheckReadAccess(IDbConnection conn)
        {
            var result = await conn.ExecuteScalarAsync<int>("SELECT 1");
            return result == 1;
        }

        private async Task<int> GetActiveConnections(IDbConnection conn)
        {
            return await conn.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM pg_stat_activity");
        }

        private async Task<string> GetPostgresVersion(IDbConnection conn)
        {
            return await conn.ExecuteScalarAsync<string>("SELECT version()");
        }
    }
}
