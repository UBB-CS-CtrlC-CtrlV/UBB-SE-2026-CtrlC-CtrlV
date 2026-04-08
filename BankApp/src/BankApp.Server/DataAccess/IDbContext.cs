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
    /// Executes a SQL query and maps the result using the provided function.
    /// </summary>
    /// <typeparam name="T">The type of the value returned by the mapping function.</typeparam>
    /// <param name="sqlStatement">The SQL statement to execute.</param>
    /// <param name="parameters">The positional parameters for the SQL statement.</param>
    /// <param name="map">A function that maps the <see cref="IDataReader"/> to the desired result.</param>
    /// <returns>The value produced by <paramref name="map"/>.</returns>
    T ExecuteQuery<T>(string sqlStatement, object[] parameters, Func<IDataReader, T> map);

    /// <summary>
    /// Executes a SQL command that does not return rows.
    /// </summary>
    /// <param name="sqlStatement">The SQL statement to execute.</param>
    /// <param name="parameters">The positional parameters for the SQL statement.</param>
    /// <returns>The number of rows affected.</returns>
    int ExecuteNonQuery(string sqlStatement, object[] parameters);
}