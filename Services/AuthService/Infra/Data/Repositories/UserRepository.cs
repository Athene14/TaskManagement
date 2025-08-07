using AuthService.Domain.Abstractions;
using AuthService.Domain.Models;
using Dapper;
using Database;
using Database.Dapper;
using System.Data;
using System.Linq.Expressions;

namespace AuthService.Infra.Data.Repositories
{
    internal class UserRepository : IUserRepository
    {
        private readonly IDatabaseConnectionFactory _connectionFactory;
        private readonly ModelToSqlMapper<UserDomain> _sqlMapper = new(TableNameConstants.UserTableName);

        private static readonly Expression<Func<UserDomain, object>>[] InsertProperties =
        {
        u => u.Id,
        u => u.Email,
        u => u.PasswordHash,
        u => u.FullName,
        u => u.CreatedAt
        };

        public UserRepository(IDatabaseConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        private async Task<IDbConnection> GetConnectionAsync()
            => await _connectionFactory.GetConnection();

        public async Task<Guid> CreateAsync(UserDomain user)
        {
            using var connection = await GetConnectionAsync();
            var sql = $@"
            INSERT INTO {_sqlMapper.TableName} ({_sqlMapper.InsertColumns(InsertProperties)})
            VALUES ({_sqlMapper.SelectValues(InsertProperties)})
            RETURNING {_sqlMapper.GetQuotedColumnName(u => u.Id)}";

            return await connection.ExecuteScalarAsync<Guid>(sql, user);
        }

        public async Task<UserDomain> GetByIdAsync(Guid id)
        {
            using var connection = await GetConnectionAsync();
            var sql = $@"
            SELECT {_sqlMapper.SelectAllColumns()} 
            FROM {_sqlMapper.TableName} 
            WHERE {_sqlMapper.WhereEquals(u => u.Id)}";

            return await connection.QuerySingleOrDefaultAsync<UserDomain>(sql, new { id });
        }

        public async Task<(bool success, UserDomain user)> TryGetByEmailAsync(string email)
        {
            using var connection = await GetConnectionAsync();
            var sql = $@"
            SELECT {_sqlMapper.SelectAllColumns()} 
            FROM {_sqlMapper.TableName} 
            WHERE {_sqlMapper.WhereEquals(u => u.Email)}";

            var user = await connection.QuerySingleOrDefaultAsync<UserDomain>(sql, new { email });
            return (user != null, user);
        }

        public async Task<(bool success, UserDomain user)> TryGetByName(string name)
        {
            using var connection = await GetConnectionAsync();
            var sql = $@"
            SELECT {_sqlMapper.SelectAllColumns()} 
            FROM {_sqlMapper.TableName} 
            WHERE {_sqlMapper.WhereEquals(u => u.FullName)}";

            var user = await connection.QuerySingleOrDefaultAsync<UserDomain>(sql, new { name });
            return (user != null, user);
        }

        public void Dispose()
        {
        }
    }
}
