using System.Linq.Expressions;
using System.Reflection;

namespace Database.Dapper
{
    public class ModelToSqlMapper<TModel>
    {
        private readonly string _tableName;
        public ModelToSqlMapper(string tableName)
        {
            _tableName = tableName;
        }

        public string TableName => $"\"{_tableName}\"";

        public string SelectAllColumns()
        {
            var properties = typeof(TModel).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            return string.Join(", ", properties.Select(p => $"\"{p.Name.ToLower()}\""));
        }

        public string GetQuotedColumnName<TProperty>(Expression<Func<TModel, TProperty>> propertyExpression)
            => $"\"{GetColumnName(propertyExpression)}\"";

        public string GetColumnName<TProperty>(Expression<Func<TModel, TProperty>> propertyExpression)
            => GetMemberInfo(propertyExpression).Name.ToLower();

        public string SelectColumns(params Expression<Func<TModel, object>>[] propertyExpressions)
            => string.Join(", ", propertyExpressions.Select(GetQuotedColumnName));

        public string SelectValues(params Expression<Func<TModel, object>>[] propertyExpressions)
            => string.Join(", ", propertyExpressions.Select(expr => $"@{GetColumnName(expr)}"));

        public string InsertColumns(params Expression<Func<TModel, object>>[] propertyExpressions)
            => SelectColumns(propertyExpressions);

        public string UpdateSetClause(params Expression<Func<TModel, object>>[] propertyExpressions)
            => string.Join(", ", propertyExpressions.Select(expr => $"{GetQuotedColumnName(expr)} = @{GetColumnName(expr)}"));

        public string WhereEquals<TProperty>(Expression<Func<TModel, TProperty>> propertyExpression)
            => $"{GetQuotedColumnName(propertyExpression)} = @{GetColumnName(propertyExpression)}";



        private MemberInfo GetMemberInfo(LambdaExpression lambda)
        {
            return lambda.Body switch
            {
                MemberExpression m => m.Member,
                UnaryExpression u when u.Operand is MemberExpression m => m.Member,
                _ => throw new ArgumentException("Invalid property expression")
            };
        }
    }
}
