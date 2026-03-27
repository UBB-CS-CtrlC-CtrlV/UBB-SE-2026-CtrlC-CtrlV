using Microsoft.Data.SqlClient;
using MovieApp.Core.Repositories;

namespace MovieApp.Infrastructure;

public sealed class SqlUserEventAttendanceRepository : IUserEventAttendanceRepository
{
    private readonly string _connectionString;

    public SqlUserEventAttendanceRepository(DatabaseOptions databaseOptions)
    {
        ArgumentNullException.ThrowIfNull(databaseOptions);
        _connectionString = databaseOptions.ConnectionString;
    }

    public async Task<IReadOnlyList<int>> GetJoinedEventIdsAsync(int userId, CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT EventId FROM dbo.UserEventAttendance WHERE UserId = @userId";

        var ids = new List<int>();

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@userId", userId);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
            ids.Add(reader.GetInt32(0));

        return ids;
    }

    public async Task JoinAsync(int userId, int eventId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            IF NOT EXISTS (
                SELECT 1 FROM dbo.UserEventAttendance
                WHERE UserId = @userId AND EventId = @eventId
            )
            INSERT INTO dbo.UserEventAttendance (UserId, EventId)
            VALUES (@userId, @eventId)";

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@userId", userId);
        command.Parameters.AddWithValue("@eventId", eventId);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
