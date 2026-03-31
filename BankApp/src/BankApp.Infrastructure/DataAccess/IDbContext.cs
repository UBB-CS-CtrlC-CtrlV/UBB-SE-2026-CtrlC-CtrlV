using Microsoft.Data.SqlClient;
using System.Data;
namespace BankApp.Infrastructure.DataAccess
{
    public interface IDbContext : IDisposable
    {
        SqlTransaction BeginTransaction();
        void CommitTransaction();
        void RollbackTransaction();
        IDataReader ExecuteQuery(string sql, object[] parameters);
        int ExecuteNonQuery(string sql, object[] parameters);
    }
}
