using System.Data;

namespace Database
{
    public interface IDatabaseConnectionFactory
    {
        Task<IDbConnection> GetConnection();
    }
}
