using Dapper;
using Database;
using Database.Dapper;
using NotificationService.Domain.Abstractions;
using NotificationService.Domain.Models;
using System.Data;
using System.Linq.Expressions;

namespace NotificationService.Infra.Data.Repositories
{
    internal class NotificationRepository : INotificationRepository
    {
        private readonly IDatabaseConnectionFactory _connectionFactory;
        private readonly ModelToSqlMapper<NotificationDomainModel> _sqlMapper = new(TableNameConstants.NotificationTableName);

        private static readonly Expression<Func<NotificationDomainModel, object>>[] InsertProperties =
        {
        n => n.Id,
        n => n.CreatedTimestamp,
        n => n.Message
        };

        public NotificationRepository(IDatabaseConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        private async Task<IDbConnection> GetConnectionAsync()
            => await _connectionFactory.GetConnection();

        public async Task<NotificationDomainModel> GetByIdAsync(Guid id)
        {
            using var connection = await GetConnectionAsync();
            var sql = $@"
            SELECT {_sqlMapper.SelectAllColumns()} 
            FROM {_sqlMapper.TableName} 
            WHERE {_sqlMapper.WhereEquals(n => n.Id)}";

            return await connection.QuerySingleOrDefaultAsync<NotificationDomainModel>(sql, new { id });
        }

        public async Task<Guid> CreateAsync(NotificationDomainModel notification)
        {
            using var connection = await GetConnectionAsync();
            var sql = $@"
            INSERT INTO {_sqlMapper.TableName} ({_sqlMapper.InsertColumns(InsertProperties)})
            VALUES ({_sqlMapper.SelectValues(InsertProperties)})
            RETURNING {_sqlMapper.GetQuotedColumnName(n => n.Id)}";

            return await connection.ExecuteScalarAsync<Guid>(sql, notification);
        }

        public async Task<IEnumerable<NotificationDomainModel>> GetManyByIds(params Guid[] ids)
        {
            if (ids == null || ids.Length == 0)
                return Enumerable.Empty<NotificationDomainModel>();

            using var connection = await GetConnectionAsync();
            var sql = $@"
            SELECT {_sqlMapper.SelectAllColumns()} 
            FROM {_sqlMapper.TableName} 
            WHERE {_sqlMapper.GetQuotedColumnName(n => n.Id)} = ANY(@Ids)";

            return await connection.QueryAsync<NotificationDomainModel>(sql, new { Ids = ids });
        }

        public void Dispose()
        {
        }
    }
}
