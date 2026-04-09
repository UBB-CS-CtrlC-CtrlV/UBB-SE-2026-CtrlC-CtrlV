using System.Data;
using Microsoft.Data.SqlClient;

namespace BankApp.Server.DataAccess;

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
    public IDbConnection GetConnection()
    {
        if (connection is not null && connection.State is not ConnectionState.Closed)
        {
            return connection;
        }

        try
        {
            connection = new SqlConnection(connectionString);
            connection.Open();
        }
        catch (SqlException sqlException)
        {
            throw new Exception($"Failed to connect to the database: {sqlException.Message}", sqlException);
        }

        return connection;
    }

    /// <inheritdoc />
    public SqlTransaction BeginTransaction()
    {
        SqlConnection activeConnection = (SqlConnection)GetConnection();
        try
        {
            currentTransaction = activeConnection.BeginTransaction();
        }
        catch (SqlException sqlException)
        {
            throw new Exception($"Failed to begin transaction: {sqlException.Message}", sqlException);
        }

        return currentTransaction;
    }

    /// <inheritdoc />
    public void CommitTransaction()
    {
        if (currentTransaction is null)
        {
            return;
        }

        currentTransaction.Commit();
        currentTransaction = null;
    }

    /// <inheritdoc />
    public void RollbackTransaction()
    {
        if (currentTransaction is null)
        {
            return;
        }

        currentTransaction.Rollback();
        currentTransaction = null;
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
}
