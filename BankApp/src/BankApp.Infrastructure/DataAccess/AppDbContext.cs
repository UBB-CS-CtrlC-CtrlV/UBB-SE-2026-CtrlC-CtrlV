using System.Data;
using ErrorOr;
using Microsoft.Data.SqlClient;

namespace BankApp.Infrastructure.DataAccess;

/// <summary>
/// Provides a concrete implementation of <see cref="IDbContext"/> using SQL Server via <see cref="SqlConnection"/>.
/// </summary>
public class AppDbContext : IDbContext
{
    private readonly string connectionString;
    private SqlConnection? connection;
    private SqlTransaction? currentTransaction;

    /// <summary>
    /// Initializes a new instance of the <see cref="AppDbContext"/> class.
    /// </summary>
    /// <param name="connectionString">The SQL Server connection string.</param>
    public AppDbContext(string connectionString)
    {
        this.connectionString = connectionString;
    }

    /// <inheritdoc />
    public ErrorOr<T> Query<T>(Func<SqlConnection, T> operation)
    {
        try
        {
            T result = operation(this.GetConnection());

            if (result is null)
            {
                return Error.NotFound(description: "No record found.");
            }

            return result;
        }
        catch (Exception ex)
        {
            return Error.Failure(description: ex.Message);
        }
    }

    /// <inheritdoc />
    public ErrorOr<SqlTransaction> BeginTransaction()
    {
        SqlConnection activeConnection = this.GetConnection();
        try
        {
            currentTransaction = activeConnection.BeginTransaction();
        }
        catch (Exception ex) when (ex is SqlException or InvalidOperationException)
        {
            return Error.Failure(description: $"Failed to begin transaction: {ex.Message}");
        }

        return currentTransaction;
    }

    /// <inheritdoc />
    public ErrorOr<Success> CommitTransaction()
    {
        if (currentTransaction is null)
        {
            return Error.Conflict(description: "No active transaction to commit.");
        }

        currentTransaction.Commit();
        currentTransaction = null;
        return Result.Success;
    }

    /// <inheritdoc />
    public ErrorOr<Success> RollbackTransaction()
    {
        if (currentTransaction is null)
        {
            return Error.Conflict(description: "No active transaction to rollback.");
        }

        currentTransaction.Rollback();
        currentTransaction = null;
        return Result.Success;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        GC.SuppressFinalize(this);

        currentTransaction?.Dispose();

        if (connection is null)
        {
            return;
        }

        if (connection.State != ConnectionState.Closed)
        {
            connection.Close();
        }

        connection.Dispose();
        connection = null;
    }

    private SqlConnection GetConnection()
    {
        if (connection is not null && connection.State is not ConnectionState.Closed)
        {
            return connection;
        }

        connection = new SqlConnection(connectionString);
        connection.Open();
        return connection;
    }
}
