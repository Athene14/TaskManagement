using Dapper;
using Database;
using Database.Dapper;
using System.Data;
using System.Linq.Expressions;
using TaskService.Domain.Abstractions;
using TaskService.Domain.Models;

namespace TaskService.Infra.Data.Repositories
{
    internal class TaskHistoryRepository : ITaskHistoryRepository
    {
        private readonly IDatabaseConnectionFactory _connectionFactory;
        private readonly ModelToSqlMapper<TaskSnapshotDomainModel> _sqlMapper = new(TableNameConstants.TaskHistoryTableName);

        private static readonly Expression<Func<TaskSnapshotDomainModel, object>>[] InsertProperties =
        {
        s => s.SnapshotId,
        s => s.TaskId,
        s => s.ChangedBy,
        s => s.ChangeTime,
        s => s.Title,
        s => s.Description,
        s => s.IsActive,
        s => s.AssignedUserId
    };

        public TaskHistoryRepository(IDatabaseConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        private async Task<IDbConnection> GetConnectionAsync()
            => await _connectionFactory.GetConnection();

        public async Task AddSnapshotAsync(TaskSnapshotDomainModel snapshot)
        {
            using var connection = await GetConnectionAsync();
            var sql = $@"
            INSERT INTO {_sqlMapper.TableName} ({_sqlMapper.InsertColumns(InsertProperties)})
            VALUES ({_sqlMapper.SelectValues(InsertProperties)})";

            await connection.ExecuteAsync(sql, snapshot);
        }

        public async Task<IEnumerable<TaskSnapshotDomainModel>> GetSnapshotsByTaskIdAsync(Guid taskId)
        {
            using var connection = await GetConnectionAsync();
            var sql = $@"
            SELECT {_sqlMapper.SelectAllColumns()} 
            FROM {_sqlMapper.TableName} 
            WHERE {_sqlMapper.WhereEquals(s => s.TaskId)}
            ORDER BY {_sqlMapper.GetQuotedColumnName(s => s.ChangeTime)} DESC";

            return await connection.QueryAsync<TaskSnapshotDomainModel>(sql, new { taskId });
        }

        public async Task<TaskSnapshotDomainModel> GetLatestSnapshotAsync(Guid taskId)
        {
            using var connection = await GetConnectionAsync();
            var sql = $@"
            SELECT {_sqlMapper.SelectAllColumns()} 
            FROM {_sqlMapper.TableName} 
            WHERE {_sqlMapper.WhereEquals(s => s.TaskId)}
            ORDER BY {_sqlMapper.GetQuotedColumnName(s => s.ChangeTime)} DESC
            LIMIT 1";

            return await connection.QuerySingleOrDefaultAsync<TaskSnapshotDomainModel>(sql, new { taskId });
        }
    }
}
