using Microsoft.Data.SqlClient;
using MovieApp.Core.Repositories;
using MovieApp.Core.Models;

namespace MovieApp.Infrastructure;

/// <summary>
/// SQL Server-backed marathon repository.
/// </summary>
public sealed class SqlMarathonRepository : IMarathonRepository
{
    private readonly string _connectionString;

    public SqlMarathonRepository(DatabaseOptions databaseOptions)
    {
        ArgumentNullException.ThrowIfNull(databaseOptions);

        _connectionString = databaseOptions.ConnectionString;
    }

    public async Task<IEnumerable<Marathon>> GetActiveMarathonsAsync()
    {
        var marathons = new List<Marathon>();
        await using var connection = new SqlConnection(_connectionString);
        await using var command = new SqlCommand(
            "SELECT Id, Title, Description, PosterUrl, Theme, PrerequisiteMarathonId, WeekScoping " +
            "FROM dbo.Marathons WHERE IsActive = 1", connection);

        await connection.OpenAsync();
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            marathons.Add(new Marathon
            {
                Id = reader.GetInt32(0),
                Title = reader.GetString(1),
                Description = reader.IsDBNull(2) ? null : reader.GetString(2),
                PosterUrl = reader.GetString(3),
                Theme = reader.IsDBNull(4) ? null : reader.GetString(4),
                PrerequisiteMarathonId = reader.IsDBNull(5) ? null : reader.GetInt32(5),
                WeekScoping = reader.IsDBNull(6) ? null : reader.GetString(6)
            });
        }
        return marathons;
    }

    public async Task<IEnumerable<MarathonProgress>> GetLeaderboardAsync(int marathonId)
    {
        var rankings = new List<MarathonProgress>();
        await using var connection = new SqlConnection(_connectionString);
        await using var command = new SqlCommand(
            "SELECT UserId, MarathonId, TriviaAccuracy, CompletedMoviesCount, FinishedAt " +
            "FROM dbo.MarathonProgress WHERE MarathonId = @MarathonId " +
            "ORDER BY TriviaAccuracy DESC, FinishedAt ASC", connection);

        command.Parameters.AddWithValue("@MarathonId", marathonId);

        await connection.OpenAsync();
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            rankings.Add(new MarathonProgress
            {
                UserId = reader.GetInt32(0),
                MarathonId = reader.GetInt32(1),
                TriviaAccuracy = reader.GetDouble(2),
                CompletedMoviesCount = reader.GetInt32(3),
                FinishedAt = reader.IsDBNull(4) ? null : reader.GetDateTime(4)
            });
        }
        return rankings;
    }

    public async Task<MarathonProgress?> GetUserProgressAsync(int userId, int marathonId)
    {
        await using var connection = new SqlConnection(_connectionString);
        await using var command = new SqlCommand(
            "SELECT UserId, MarathonId, TriviaAccuracy, CompletedMoviesCount, FinishedAt " +
            "FROM dbo.MarathonProgress WHERE UserId = @UserId AND MarathonId = @MarathonId", connection);

        command.Parameters.AddWithValue("@UserId", userId);
        command.Parameters.AddWithValue("@MarathonId", marathonId);

        await connection.OpenAsync();
        await using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new MarathonProgress
            {
                UserId = reader.GetInt32(0),
                MarathonId = reader.GetInt32(1),
                TriviaAccuracy = reader.GetDouble(2),
                CompletedMoviesCount = reader.GetInt32(3),
                FinishedAt = reader.IsDBNull(4) ? null : reader.GetDateTime(4)
            };
        }
        return null;
    }

    public async Task<bool> JoinMarathonAsync(int userId, int marathonId)
    {
        await using var connection = new SqlConnection(_connectionString);
        await using var command = new SqlCommand(
            "INSERT INTO dbo.MarathonProgress (UserId, MarathonId, JoinedAt) " +
            "VALUES (@UserId, @MarathonId, @JoinedAt)", connection);

        command.Parameters.AddWithValue("@UserId", userId);
        command.Parameters.AddWithValue("@MarathonId", marathonId);
        command.Parameters.AddWithValue("@JoinedAt", DateTime.Now);

        await connection.OpenAsync();
        return await command.ExecuteNonQueryAsync() > 0;
    }

    public async Task<bool> UpdateProgressAsync(MarathonProgress progress)
    {
        await using var connection = new SqlConnection(_connectionString);
        await using var command = new SqlCommand(
            "UPDATE dbo.MarathonProgress SET " +
            "TriviaAccuracy = @Accuracy, CompletedMoviesCount = @Count, FinishedAt = @FinishedAt " +
            "WHERE UserId = @UserId AND MarathonId = @MarathonId", connection);

        command.Parameters.AddWithValue("@Accuracy", progress.TriviaAccuracy);
        command.Parameters.AddWithValue("@Count", progress.CompletedMoviesCount);
        command.Parameters.AddWithValue("@FinishedAt", (object?)progress.FinishedAt ?? DBNull.Value);
        command.Parameters.AddWithValue("@UserId", progress.UserId);
        command.Parameters.AddWithValue("@MarathonId", progress.MarathonId);

        await connection.OpenAsync();
        return await command.ExecuteNonQueryAsync() > 0;
    }
    public async Task<bool> IsPrerequisiteCompletedAsync(
    int userId, int prerequisiteMarathonId)
    {
        const string sql = """
        SELECT COUNT(1) FROM dbo.MarathonProgress
        WHERE UserId = @userId
          AND MarathonId = @prereqId
          AND FinishedAt IS NOT NULL;
        """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@userId", userId);
        command.Parameters.AddWithValue("@prereqId", prerequisiteMarathonId);
        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result) > 0;
    }

    public async Task<int> GetMarathonMovieCountAsync(int marathonId)
    {
        const string sql = """
        SELECT COUNT(1) FROM dbo.MarathonMovies
        WHERE MarathonId = @marathonId;
        """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@marathonId", marathonId);
        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }
}
