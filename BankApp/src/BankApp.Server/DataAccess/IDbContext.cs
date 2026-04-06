using System.Data;
using Microsoft.Data.SqlClient;

namespace BankApp.Server.DataAccess;

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
    /// <param name="sqlStatement">The SQL statement to execute.</param>
    /// <param name="parameters">The positional parameters for the SQL statement.</param>
    /// <param name="map"></param>
    /// <returns>An <see cref="IDataReader"/> containing the query results.</returns>
    T ExecuteQuery<T>(string sqlStatement, object[] parameters, Func<IDataReader, T> map);

    /// <summary>
    /// Executes a SQL command that does not return rows.
    /// </summary>
    /// <param name="sql">The SQL statement to execute.</param>
    /// <param name="parameters">The positional parameters for the SQL statement.</param>
    /// <returns>The number of rows affected.</returns>
    int ExecuteNonQuery(string sql, object[] parameters);
}