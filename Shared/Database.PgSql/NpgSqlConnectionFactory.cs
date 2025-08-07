using Npgsql;
using System.Data;

namespace Database.Dapper
{
    public class NpgSqlConnectionFactory : IDatabaseConnectionFactory
    {
        private readonly NpgsqlDataSource _source;

        public NpgSqlConnectionFactory(NpgsqlDataSource dataSource)
        {
            _source = dataSource;
        }

        public async Task<IDbConnection> GetConnection()
        {
            var connection = await _source.OpenConnectionAsync();
            return connection;
        }
    }
}
