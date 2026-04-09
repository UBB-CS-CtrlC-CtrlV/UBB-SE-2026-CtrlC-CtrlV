using System.Data;
using Microsoft.Data.SqlClient;

namespace BankApp.Server.DataAccess;

/// <summary>
/// Provides an abstraction over the database connection, supporting transactions and Dapper-based queries.
/// </summary>
public interface IDbContext : IDisposable
{
    /// <summary>
    /// Gets an open database connection for use with Dapper extension methods.
    /// </summary>
    /// <returns>An open <see cref="IDbConnection"/> instance.</returns>
    IDbConnection GetConnection();

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
}
