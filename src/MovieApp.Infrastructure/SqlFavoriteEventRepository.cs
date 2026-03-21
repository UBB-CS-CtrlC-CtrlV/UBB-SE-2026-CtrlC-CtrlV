using Microsoft.Data.SqlClient;
using MovieApp.Core.Models;
using MovieApp.Core.Repositories;

namespace MovieApp.Infrastructure;

public sealed class SqlFavoriteEventRepository : IFavoriteEventRepository
{
    private readonly string _connectionString;

    public SqlFavoriteEventRepository(DatabaseOptions databaseOptions)
    {
        _connectionString = databaseOptions.ConnectionString;
    }

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
