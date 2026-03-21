using Microsoft.Data.SqlClient;
using MovieApp.Core.Models;
using MovieApp.Core.Repositories;

namespace MovieApp.Infrastructure;

public sealed class SqlNotificationRepository : INotificationRepository
{
    private readonly string _connectionString;

    public SqlNotificationRepository(DatabaseOptions databaseOptions)
    {
        _connectionString = databaseOptions.ConnectionString;
    }

    public async Task AddAsync(Notification notification, CancellationToken cancellationToken = default)
    {
        const string sql = """
            INSERT INTO dbo.Notifications (UserId, EventId, Type, Message, State, CreatedAt)
            VALUES (@userId, @eventId, @type, @message, @state, @createdAt);
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@userId", notification.UserId);
        command.Parameters.AddWithValue("@eventId", notification.EventId);
        command.Parameters.AddWithValue("@type", notification.Type);
        command.Parameters.AddWithValue("@message", notification.Message);
        command.Parameters.AddWithValue("@state", notification.State.ToString());
        command.Parameters.AddWithValue("@createdAt", notification.CreatedAt);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task RemoveAsync(int notificationId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            DELETE FROM dbo.Notifications
            WHERE Id = @id;
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", notificationId);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Notification>> FindByUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT Id, UserId, EventId, Type, Message, State, CreatedAt
            FROM dbo.Notifications
            WHERE UserId = @userId
            ORDER BY CreatedAt DESC;
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@userId", userId);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var results = new List<Notification>();

        while (await reader.ReadAsync(cancellationToken))
        {
            var stateText = reader.GetString(5);
            _ = Enum.TryParse<NotificationState>(stateText, ignoreCase: true, out var state);

            results.Add(new Notification
            {
                Id = reader.GetInt32(0),
                UserId = reader.GetInt32(1),
                EventId = reader.GetInt32(2),
                Type = reader.GetString(3),
                Message = reader.GetString(4),
                State = state,
                CreatedAt = reader.GetDateTime(6),
            });
        }

        return results;
    }
}
