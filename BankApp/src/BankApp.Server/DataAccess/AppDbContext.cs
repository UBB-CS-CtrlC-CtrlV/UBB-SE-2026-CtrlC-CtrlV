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

    /// <summary>
    /// Gets an open <see cref="SqlConnection"/>, creating one if necessary.
    /// </summary>
    /// <returns>An open <see cref="SqlConnection"/> instance.</returns>
    private SqlConnection GetConnection()
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
        SqlConnection activeConnection = GetConnection();
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

    /// <summary>
    /// Gets the currently active transaction, or <see langword="null"/> if none exists.
    /// </summary>
    /// <returns>The current <see cref="SqlTransaction"/>, or <see langword="null"/>.</returns>
    private SqlTransaction? GetCurrentTransaction()
    {
        return currentTransaction;
    }

    private static void AddParameters(SqlCommand command, object[] parameters)
    {
        for (var parameterIndex = 0; parameterIndex < parameters.Length; parameterIndex++)
        {
            command.Parameters.AddWithValue($"@p{parameterIndex}", parameters[parameterIndex]);
        }
    }

    /// <inheritdoc />
    public T ExecuteQuery<T>(string sqlStatement, object[] parameters, Func<IDataReader, T> map)
    {
        var activeConnection = this.GetConnection();
        using var command = new SqlCommand(sqlStatement, activeConnection, this.currentTransaction);
        AppDbContext.AddParameters(command, parameters);
        using var reader = command.ExecuteReader();
        return map(reader);
    }

    /// <inheritdoc />
    public int ExecuteNonQuery(string sqlStatement, object[] parameters)
    {
        var activeConnection = GetConnection();
        using var command = new SqlCommand(sqlStatement, activeConnection, currentTransaction);
        AddParameters(command, parameters);
        return command.ExecuteNonQuery();
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