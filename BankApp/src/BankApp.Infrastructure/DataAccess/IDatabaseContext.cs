using System.Data;
using ErrorOr;
using Microsoft.Data.SqlClient;

namespace BankApp.Infrastructure.DataAccess;

/// <summary>
/// Provides an abstraction over the database connection, supporting transactions and safe query execution.
/// </summary>
public interface IDatabaseContext : IDisposable
{
    /// <summary>
    /// Executes a database operation using an open connection, returning the result as <see cref="ErrorOr{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of the value returned by the operation.</typeparam>
    /// <param name="operation">A function that receives an open <see cref="SqlConnection"/> and returns a value.</param>
    /// <returns>The result of the operation, or <see cref="Error.Failure"/> if an exception occurred.</returns>
    /// <remarks>
    /// Any exception thrown during connection or query execution is caught and returned as <see cref="Error.Failure"/>.
    /// </remarks>
    ErrorOr<T> Query<T>(Func<SqlConnection, T> operation);

    /// <summary>
    /// Begins a new database transaction.
    /// </summary>
    /// <returns>The <see cref="SqlTransaction"/> that was started.</returns>
    ErrorOr<SqlTransaction> BeginTransaction();

    /// <summary>
    /// Commits the current database transaction.
    /// </summary>
    ErrorOr<Success> CommitTransaction();

    /// <summary>
    /// Rolls back the current database transaction.
    /// </summary>
    ErrorOr<Success> RollbackTransaction();
}
