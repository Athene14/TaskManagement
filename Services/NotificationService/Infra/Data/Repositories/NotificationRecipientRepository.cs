using Dapper;
using Database;
using Database.Dapper;
using NotificationService.Domain.Abstractions;
using NotificationService.Domain.Models;
using System.Data;
using System.Linq.Expressions;

namespace NotificationService.Infra.Data.Repositories
{
    internal class NotificationRecipientRepository : INotificationRecipientRepository
    {
        private readonly IDatabaseConnectionFactory _connectionFactory;
        private readonly ModelToSqlMapper<NotificationRecipientDomainModel> _sqlMapper = new(TableNameConstants.NotificationRecipientsTableName);

        private static readonly Expression<Func<NotificationRecipientDomainModel, object>>[] InsertProperties =
        {
        r => r.Id,
        r => r.NotificationId,
        r => r.RecipientId,
        r => r.IsRead
    };

        public NotificationRecipientRepository(IDatabaseConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        private async Task<IDbConnection> GetConnectionAsync()
            => await _connectionFactory.GetConnection();

        public async Task AddRecipientAsync(NotificationRecipientDomainModel recipient)
        {
            using var connection = await GetConnectionAsync();
            var sql = $@"
            INSERT INTO {_sqlMapper.TableName} ({_sqlMapper.InsertColumns(InsertProperties)})
            VALUES ({_sqlMapper.SelectValues(InsertProperties)})";

            await connection.ExecuteAsync(sql, recipient);
        }

        public async Task AddRecipientsAsync(IEnumerable<NotificationRecipientDomainModel> recipients)
        {
            var recipientList = recipients.ToList();
            if (!recipientList.Any())
                return;

            using var connection = await GetConnectionAsync();
            var sql = $@"
            INSERT INTO {_sqlMapper.TableName} ({_sqlMapper.InsertColumns(InsertProperties)})
            VALUES ({_sqlMapper.SelectValues(InsertProperties)})";

            await connection.ExecuteAsync(sql, recipientList);
        }

        public async Task<IEnumerable<NotificationRecipientDomainModel>> GetByUserIdAsync(Guid userId, bool unreadOnly)
        {
            using var connection = await GetConnectionAsync();
            var whereClause = unreadOnly
                ? $"{_sqlMapper.WhereEquals(r => r.RecipientId)} AND {_sqlMapper.GetQuotedColumnName(r => r.IsRead)} = false"
                : _sqlMapper.WhereEquals(r => r.RecipientId);

            var sql = $@"
            SELECT {_sqlMapper.SelectAllColumns()} 
            FROM {_sqlMapper.TableName} 
            WHERE {whereClause}";

            return await connection.QueryAsync<NotificationRecipientDomainModel>(sql, new { RecipientId = userId });
        }

        public async Task<IEnumerable<NotificationRecipientDomainModel>> GetRecipientsForNotificationAsync(Guid notificationId)
        {
            using var connection = await GetConnectionAsync();
            var sql = $@"
            SELECT {_sqlMapper.SelectAllColumns()} 
            FROM {_sqlMapper.TableName} 
            WHERE {_sqlMapper.WhereEquals(r => r.NotificationId)}";

            return await connection.QueryAsync<NotificationRecipientDomainModel>(sql, new { notificationId });
        }

        public async Task MarkAsReadAsync(Guid recipientId, Guid notificationId)
        {
            using var connection = await GetConnectionAsync();
            var sql = $@"
            UPDATE {_sqlMapper.TableName} 
            SET {_sqlMapper.GetQuotedColumnName(r => r.IsRead)} = true 
            WHERE {_sqlMapper.WhereEquals(r => r.RecipientId)} 
            AND {_sqlMapper.WhereEquals(r => r.NotificationId)}";

            await connection.ExecuteAsync(sql, new { RecipientId = recipientId, NotificationId = notificationId });
        }

        public async Task<NotificationRecipientDomainModel> GetByRecipientAndNotificationAsync(Guid recipientId, Guid notificationId)
        {
            using var connection = await GetConnectionAsync();
            var sql = $@"
            SELECT {_sqlMapper.SelectAllColumns()} 
            FROM {_sqlMapper.TableName} 
            WHERE {_sqlMapper.WhereEquals(r => r.RecipientId)} 
            AND {_sqlMapper.WhereEquals(r => r.NotificationId)}";

            return await connection.QuerySingleOrDefaultAsync<NotificationRecipientDomainModel>(
                sql,
                new { RecipientId = recipientId, NotificationId = notificationId });
        }

        public void Dispose()
        {
        }
    }
}
