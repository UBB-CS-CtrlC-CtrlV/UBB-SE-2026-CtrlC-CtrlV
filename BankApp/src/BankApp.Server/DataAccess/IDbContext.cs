using System.Data;
using Microsoft.Data.SqlClient;

namespace BankApp.Infrastructure.DataAccess
{
    /// <summary>
    /// Provides an abstraction over the database connection, supporting transactions and command execution.
    /// </summary>
    public interface IDbContext : IDisposable
    {
        /// <summary>
        /// Begins a new database transaction.
        /// </summary>
        /// <returns>The <see cref="SqlTransaction"/> that was started.</returns>
        SqlTransaction BeginTransaction();

        /// <summary>
        /// Commits the current database transaction.
        /// </summary>
        void CommitTransaction();

        /// <summary>
        /// Rolls back the current database transaction.
        /// </summary>
        void RollbackTransaction();

        /// <summary>
        /// Executes a SQL query and returns the resulting data reader.
        /// </summary>
        /// <param name="sql">The SQL statement to execute.</param>
        /// <param name="parameters">The positional parameters for the SQL statement.</param>
        /// <returns>An <see cref="IDataReader"/> containing the query results.</returns>
        IDataReader ExecuteQuery(string sql, object[] parameters);

        /// <summary>
        /// Executes a SQL command that does not return rows.
        /// </summary>
        /// <param name="sql">The SQL statement to execute.</param>
        /// <param name="parameters">The positional parameters for the SQL statement.</param>
        /// <returns>The number of rows affected.</returns>
        int ExecuteNonQuery(string sql, object[] parameters);
    }
}
