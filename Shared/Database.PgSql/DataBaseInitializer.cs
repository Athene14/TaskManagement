using FluentMigrator.Runner;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Database.Dapper
{
    internal class DataBaseInitializer
    {
        private readonly ILogger<DataBaseInitializer> _log;
        private IDatabaseConfiguration _config;

        public DataBaseInitializer(IServiceProvider serviceProvider)
        {
            _log = serviceProvider.GetRequiredService<ILogger<DataBaseInitializer>>();
            _config = serviceProvider.GetRequiredService<IDatabaseConfiguration>();
        }

        public async Task InitDatabase()
        {
            _log.LogInformation("Initializing database...");

            // Получаем строку подключения из параметров
            var builderString = new NpgsqlConnectionStringBuilder(_config.GetConnectionString());
            var dbName = builderString.Database;
            builderString.Database = null;

            // Удаляем из строки базу данных
            var connectionString = builderString.ConnectionString;

            // Проверка существования БД и её создание
            _log.LogInformation("Checking if database {DatabaseName} exists...", dbName);
            var isDbExist = await IsDatabaseExist(connectionString, dbName);

            if (!isDbExist)
            {
                _log.LogInformation("Database {DatabaseName} does not exist. Creating...", dbName);
                await CreateDatabase(connectionString, dbName);
                _log.LogInformation("Database {DatabaseName} successfully created", dbName);
            }
            else
            {
                _log.LogInformation("Database {DatabaseName} already exists", dbName);
            }

            // Выполнение миграций
            if (_config.Migrate())
            {
                _log.LogInformation("Applying database migrations...");
                MigrateDatabase(_config.GetConnectionString());
                _log.LogInformation("Database migrations completed successfully");
            }
            else
            {
                _log.LogDebug("Migrations are disabled in configuration. Skipping database migrations.");
            }
        }

        private async Task<bool> IsDatabaseExist(string connectionString, string dbName)
        {
            var sql = $"SELECT 1 FROM pg_database WHERE datname = '{dbName}'";
            try
            {
                var result = await ExecuteScalarSqlAsync(connectionString, sql);
                return result != null;
            }
            catch (Exception e)
            {
                _log.LogError(e, "Failed to check existence of database {DatabaseName}", dbName);
                throw new ApplicationException($"Database existence check failed for {dbName}", e);
            }
        }

        private async Task CreateDatabase(string connectionString, string dbName)
        {
            var sql = $"CREATE DATABASE \"{dbName}\";";
            try
            {
                await ExecuteSqlAsync(connectionString, sql);
            }
            catch (Exception e)
            {
                _log.LogError(e, "Failed to create database {DatabaseName}", dbName);
                throw new ApplicationException($"Database creation failed for {dbName}", e);
            }
        }

        private static async Task<object> ExecuteScalarSqlAsync(string connectionString, string sql)
        {
            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync();
            await using var command = new NpgsqlCommand(sql, conn);
            return await command.ExecuteScalarAsync();
        }

        private static async Task ExecuteSqlAsync(string connectionString, string sql)
        {
            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync();
            await using var command = new NpgsqlCommand(sql, conn);
            await command.ExecuteNonQueryAsync();
        }

        private void MigrateDatabase(string connectionString)
        {
            try
            {
                var serviceProvider = CreateMigrationServices(connectionString);
                using var scope = serviceProvider.CreateScope();
                var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();

                _log.LogDebug("Starting migrations...");
                runner.MigrateUp();
                _log.LogDebug("Migrations executed successfully");
            }
            catch (Exception e)
            {
                _log.LogError(e, "Database migration failed");
                throw new ApplicationException("Database migration failed", e);
            }
        }

        private IServiceProvider CreateMigrationServices(string connectionString)
        {
            return new ServiceCollection()
                .AddFluentMigratorCore()
                .ConfigureRunner(rb => rb
                    .AddPostgres()
                    .WithGlobalConnectionString(connectionString)
                    .ScanIn(AppDomain.CurrentDomain.GetAssemblies()).For.Migrations())
                .AddLogging(lb => lb
                    .AddFluentMigratorConsole()
                    .AddFilter("FluentMigrator", LogLevel.Information))
                .BuildServiceProvider(false);
        }
    }
}
