using Database.Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Database.Dapper
{
    public static class PgSqlDatabaseStartupExtention
    {
        public static IServiceCollection AddPgSqlDatabase(this IServiceCollection collection, string serviceConfigurationSectionName)
        {
            collection.AddSingleton<IDatabaseConfiguration, DatabaseConfiguration>((ctx) =>
            {
                var config = ctx.GetRequiredService<IConfiguration>();
                return new DatabaseConfiguration(config, serviceConfigurationSectionName);
            });
            collection.AddSingleton(ctx =>
            {
                var config = ctx.GetRequiredService<IDatabaseConfiguration>();

                var builder = new NpgsqlDataSourceBuilder(config.GetConnectionString())
                            .UseLoggerFactory(ctx.GetRequiredService<ILoggerFactory>()).EnableParameterLogging(false);

                return builder.Build();
            });
            collection.AddTransient<IDatabaseConnectionFactory, NpgSqlConnectionFactory>();

            return collection;
        }

        public static async Task InitDataBase(this IHost host)
        {
            var initializer = new DataBaseInitializer(host.Services);

            await initializer.InitDatabase();
        }
    }
}
