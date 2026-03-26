using Microsoft.Data.SqlClient;
using MovieApp.Core.Models;
using MovieApp.Core.Repositories;

namespace MovieApp.Infrastructure;

/// <summary>
/// SQL Server implementation of <see cref="IFavoriteEventRepository"/>.
/// </summary>
public sealed class SqlFavoriteEventRepository : IFavoriteEventRepository
{
    private readonly string _connectionString;

    /// <summary>
    /// Creates the repository using the configured database connection string.
    /// </summary>
    public SqlFavoriteEventRepository(DatabaseOptions databaseOptions)
    {
        _connectionString = databaseOptions.ConnectionString;
    }

    /// <inheritdoc />
    public async Task AddAsync(int userId, int eventId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            INSERT INTO dbo.FavoriteEvents (UserId, EventId)
            VALUES (@userId, @eventId);
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@userId", userId);
        command.Parameters.AddWithValue("@eventId", eventId);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task RemoveAsync(int userId, int eventId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            DELETE FROM dbo.FavoriteEvents
            WHERE UserId = @userId
              AND EventId = @eventId;
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@userId", userId);
        command.Parameters.AddWithValue("@eventId", eventId);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<FavoriteEvent>> FindByUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT Id, UserId, EventId, CreatedAt
            FROM dbo.FavoriteEvents
            WHERE UserId = @userId
            ORDER BY CreatedAt DESC;
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@userId", userId);

        return await ReadFavoriteEventsAsync(command, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> ExistsAsync(int userId, int eventId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT TOP 1 1
            FROM dbo.FavoriteEvents
            WHERE UserId = @userId
              AND EventId = @eventId;
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@userId", userId);
        command.Parameters.AddWithValue("@eventId", eventId);

        var result = await command.ExecuteScalarAsync(cancellationToken);
        return result is not null;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<int>> GetUsersByFavoriteEventAsync(int eventId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT UserId
            FROM dbo.FavoriteEvents
            WHERE EventId = @eventId;
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@eventId", eventId);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var results = new List<int>();

        while (await reader.ReadAsync(cancellationToken))
        {
            results.Add(reader.GetInt32(0));
        }

        return results;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<FavoriteEvent>> FindByEventAsync(int eventId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT Id, UserId, EventId, CreatedAt
            FROM dbo.FavoriteEvents
            WHERE EventId = @eventId
            ORDER BY CreatedAt DESC;
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@eventId", eventId);

        return await ReadFavoriteEventsAsync(command, cancellationToken);
    }

    private static async Task<IReadOnlyList<FavoriteEvent>> ReadFavoriteEventsAsync(SqlCommand command, CancellationToken cancellationToken)
    {
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var results = new List<FavoriteEvent>();

        while (await reader.ReadAsync(cancellationToken))
        {
            results.Add(new FavoriteEvent
            {
                Id = reader.GetInt32(0),
                UserId = reader.GetInt32(1),
                EventId = reader.GetInt32(2),
                CreatedAt = reader.GetDateTime(3),
            });
        }

        return results;
    }
}
