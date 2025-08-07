using Dapper;
using Database;
using Database.Dapper;
using System.Data;
using System.Linq.Expressions;
using TaskManagementServices.Shared.TaskService.DTO;
using TaskService.Domain.Abstractions;
using TaskService.Domain.Models;

namespace TaskService.Infra.Data.Repositories
{
    internal class TaskRepository : ITaskRepository
    {

        private readonly IDatabaseConnectionFactory _connectionFactory;
        private readonly ModelToSqlMapper<TaskDomainModel> _sqlMapper = new(TableNameConstants.TaskTableName);

        private static readonly Expression<Func<TaskDomainModel, object>>[] InsertProperties =
        {
        t => t.TaskId,
        t => t.CreatedBy,
        t => t.Title,
        t => t.Description,
        t => t.CreatedAt,
        t => t.UpdatedAt,
        t => t.IsActive,
        t => t.AssignedUserId
    };

        private static readonly Expression<Func<TaskDomainModel, object>>[] UpdateProperties =
        {
        t => t.Title,
        t => t.Description,
        t => t.UpdatedAt,
        t => t.IsActive,
        t => t.AssignedUserId
    };

        public TaskRepository(IDatabaseConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        private async Task<IDbConnection> GetConnectionAsync()
            => await _connectionFactory.GetConnection();

        public async Task<PagedResponse<TaskDomainModel>> GetTasksAsync(TaskFilter filter, int page, int pageSize)
        {
            using var connection = await GetConnectionAsync();

            var whereClauses = new List<string>();
            var parameters = new DynamicParameters();

            // Опциональные фильтры
            if (filter.CreatedBy.HasValue && filter.CreatedBy != Guid.Empty)
            {
                whereClauses.Add(_sqlMapper.WhereEquals(t => t.CreatedBy));
                parameters.Add("CreatedBy", filter.CreatedBy);
            }

            if (!string.IsNullOrEmpty(filter.Title))
            {
                whereClauses.Add($"{_sqlMapper.GetQuotedColumnName(t => t.Title)} ILIKE @Title");
                parameters.Add("Title", $"%{filter.Title}%");
            }

            if (filter.AssignedUserId.HasValue && filter.AssignedUserId.Value != Guid.Empty)
            {
                whereClauses.Add(_sqlMapper.WhereEquals(t => t.AssignedUserId));
                parameters.Add("AssignedUserId", filter.AssignedUserId.Value);
            }

            if (filter.CreatedFromTimestamp.HasValue)
            {
                whereClauses.Add($"{_sqlMapper.GetQuotedColumnName(t => t.CreatedAt)} >= @CreatedFrom");
                parameters.Add("CreatedFrom", filter.CreatedFromTimestamp.Value);
            }

            if (filter.CreatedToTimestamp.HasValue)
            {
                whereClauses.Add($"{_sqlMapper.GetQuotedColumnName(t => t.CreatedAt)} <= @CreatedTo");
                parameters.Add("CreatedTo", filter.CreatedToTimestamp.Value);
            }

            if (filter.OnlyActive.HasValue && filter.OnlyActive.Value)
            {
                whereClauses.Add($"{_sqlMapper.GetQuotedColumnName(t => t.IsActive)} = true");
            }

            // Построение условий WHERE
            var where = whereClauses.Count > 0 ? $"WHERE {string.Join(" AND ", whereClauses)}" : "";

            // Запрос для общего количества
            var countSql = $"SELECT COUNT(*) FROM {_sqlMapper.TableName} {where}";
            var totalCount = await connection.ExecuteScalarAsync<int>(countSql, parameters);

            // Запрос данных с пагинацией
            var dataSql = $@"
            SELECT {_sqlMapper.SelectAllColumns()} 
            FROM {_sqlMapper.TableName} 
            {where}
            ORDER BY {_sqlMapper.GetQuotedColumnName(t => t.CreatedAt)} DESC
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

            parameters.Add("Offset", (page - 1) * pageSize);
            parameters.Add("PageSize", pageSize);

            var items = (await connection.QueryAsync<TaskDomainModel>(dataSql, parameters)).ToList();

            return new PagedResponse<TaskDomainModel>(items, page, pageSize, totalCount);
        }

        public async Task<TaskDomainModel> GetTaskByIdAsync(Guid taskId)
        {
            using var connection = await GetConnectionAsync();
            var sql = $@"
            SELECT {_sqlMapper.SelectAllColumns()} 
            FROM {_sqlMapper.TableName} 
            WHERE {_sqlMapper.WhereEquals(t => t.TaskId)}";

            return await connection.QuerySingleOrDefaultAsync<TaskDomainModel>(sql, new { taskId });
        }

        public async Task<Guid> CreateTaskAsync(TaskDomainModel task)
        {
            using var connection = await GetConnectionAsync();
            var sql = $@"
            INSERT INTO {_sqlMapper.TableName} ({_sqlMapper.InsertColumns(InsertProperties)})
            VALUES ({_sqlMapper.SelectValues(InsertProperties)})
            RETURNING {_sqlMapper.GetQuotedColumnName(t => t.TaskId)}";

            return await connection.ExecuteScalarAsync<Guid>(sql, task);
        }

        public async Task<bool> UpdateTaskAsync(TaskDomainModel task)
        {
            using var connection = await GetConnectionAsync();
            var sql = $@"
            UPDATE {_sqlMapper.TableName} 
            SET {_sqlMapper.UpdateSetClause(UpdateProperties)} 
            WHERE {_sqlMapper.WhereEquals(t => t.TaskId)}";

            var affected = await connection.ExecuteAsync(sql, task);
            return affected > 0;
        }

        public async Task<bool> SoftDeleteTaskAsync(Guid taskId)
        {
            using var connection = await GetConnectionAsync();
            var sql = $@"
            UPDATE {_sqlMapper.TableName} 
            SET {_sqlMapper.GetQuotedColumnName(t => t.IsActive)} = false 
            WHERE {_sqlMapper.WhereEquals(t => t.TaskId)}";

            var affected = await connection.ExecuteAsync(sql, new { taskId });
            return affected > 0;
        }
    }
}
