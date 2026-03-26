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
        const string sql = """
    SELECT UserId, MarathonId, TriviaAccuracy, CompletedMoviesCount, FinishedAt
    FROM dbo.MarathonProgress
    WHERE MarathonId = @MarathonId
    ORDER BY CompletedMoviesCount DESC,
             TriviaAccuracy DESC,
             FinishedAt ASC;
    """; 
        await using var command = new SqlCommand(sql, connection);

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

    private static (string weekString, DateTime weekStart, DateTime weekEnd) GetCurrentWeekBounds()
    {
        var now = DateTime.UtcNow;
        var weekString = $"{now.Year}-W" +
            System.Globalization.ISOWeek.GetWeekOfYear(now).ToString("D2");

        // Monday = start, Sunday 23:59:59 = end
        var daysFromMonday = ((int)now.DayOfWeek + 6) % 7;
        var monday = now.Date.AddDays(-daysFromMonday);
        var sunday = monday.AddDays(6).AddHours(23).AddMinutes(59).AddSeconds(59);

        return (weekString, monday, sunday);
    }

    public async Task<IEnumerable<Marathon>> GetWeeklyMarathonsForUserAsync(
    int userId, string weekString)
    {
        var (_, _, weekEnd) = GetCurrentWeekBounds();

        // If it's past Sunday night, this week is over — return nothing
        if (DateTime.UtcNow > weekEnd)
            return [];

        const string sql = """
        SELECT Id, Title, Description, PosterUrl, Theme,
               PrerequisiteMarathonId, WeekScoping
        FROM dbo.Marathons
        WHERE IsActive = 1
          AND WeekScoping = @week
          AND Id NOT IN (
              SELECT MarathonId FROM dbo.MarathonProgress
              WHERE UserId = @userId
                AND FinishedAt IS NOT NULL
          );
        """;

        var marathons = new List<Marathon>();
        await using var connection = new SqlConnection(_connectionString);
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@week", weekString);
        command.Parameters.AddWithValue("@userId", userId);

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
                WeekScoping = reader.IsDBNull(6) ? null : reader.GetString(6),
            });
        }
        return marathons;
    }
    public async Task AssignWeeklyMarathonsAsync(
    int userId, string weekString, int count = 10)
    {
        // Pick 'count' random marathons that:
        // 1. The user has never completed
        // 2. Don't already have a WeekScoping for this week
        const string selectSql = """
        SELECT TOP (@count) Id
        FROM dbo.Marathons
        WHERE Id NOT IN (
            SELECT MarathonId FROM dbo.MarathonProgress
            WHERE UserId = @userId AND FinishedAt IS NOT NULL
        )
        AND (WeekScoping IS NULL OR WeekScoping <> @week)
        AND PrerequisiteMarathonId IS NULL
        ORDER BY NEWID();
        """;

        var ids = new List<int>();

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var selectCmd = new SqlCommand(selectSql, connection);
        selectCmd.Parameters.AddWithValue("@count", count);
        selectCmd.Parameters.AddWithValue("@userId", userId);
        selectCmd.Parameters.AddWithValue("@week", weekString);

        await using var reader = await selectCmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
            ids.Add(reader.GetInt32(0));

        await reader.CloseAsync();

        if (ids.Count == 0) return;

        // Deactivate previous week's marathons
        const string deactivateSql = """
        UPDATE dbo.Marathons
        SET IsActive = 0
        WHERE IsActive = 1
          AND WeekScoping <> @week;
        """;

        await using var deactivateCmd = new SqlCommand(deactivateSql, connection);
        deactivateCmd.Parameters.AddWithValue("@week", weekString);
        await deactivateCmd.ExecuteNonQueryAsync();

        // Stamp the selected marathons with this week and activate them
        foreach (var id in ids)
        {
            const string updateSql = """
            UPDATE dbo.Marathons
            SET WeekScoping = @week, IsActive = 1
            WHERE Id = @id;
            """;

            await using var updateCmd = new SqlCommand(updateSql, connection);
            updateCmd.Parameters.AddWithValue("@week", weekString);
            updateCmd.Parameters.AddWithValue("@id", id);
            await updateCmd.ExecuteNonQueryAsync();
        }
    }
    public async Task<IEnumerable<MovieApp.Core.Models.Movie.Movie>> GetMoviesForMarathonAsync(
    int marathonId)
    {
        const string sql = """
        SELECT m.Id, m.Title, m.Description, m.ReleaseYear, m.DurationMinutes
        FROM dbo.Movies m
        INNER JOIN dbo.MarathonMovies mm ON m.Id = mm.MovieId
        WHERE mm.MarathonId = @marathonId
        ORDER BY m.Title;
        """;

        var movies = new List<MovieApp.Core.Models.Movie.Movie>();
        await using var connection = new SqlConnection(_connectionString);
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@marathonId", marathonId);

        await connection.OpenAsync();
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            movies.Add(new MovieApp.Core.Models.Movie.Movie
            {
                Id = reader.GetInt32(0),
                Title = reader.GetString(1),
                Description = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                ReleaseYear = reader.IsDBNull(3) ? 0 : reader.GetInt32(3),
                DurationMinutes = reader.IsDBNull(4) ? 0 : reader.GetInt32(4),
            });
        }
        return movies;
    }

    public async Task<IEnumerable<LeaderboardEntry>> GetLeaderboardWithUsernamesAsync(
        int marathonId)
    {
        const string sql = """
        SELECT u.Id, u.Username, mp.CompletedMoviesCount, mp.TriviaAccuracy, mp.FinishedAt
        FROM dbo.MarathonProgress mp
        INNER JOIN dbo.Users u ON mp.UserId = u.Id
        WHERE mp.MarathonId = @marathonId
        ORDER BY mp.CompletedMoviesCount DESC,
                 mp.TriviaAccuracy DESC,
                 mp.FinishedAt ASC;
        """;

        var entries = new List<LeaderboardEntry>();
        await using var connection = new SqlConnection(_connectionString);
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@marathonId", marathonId);

        await connection.OpenAsync();
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            entries.Add(new LeaderboardEntry
            {
                UserId = reader.GetInt32(0),
                Username = reader.GetString(1),
                CompletedMoviesCount = reader.GetInt32(2),
                TriviaAccuracy = reader.GetDouble(3),
                FinishedAt = reader.IsDBNull(4) ? null : reader.GetDateTime(4),
            });
        }
        return entries;
    }
    public async Task<int> GetParticipantCountAsync(int marathonId)
    {
        const string sql = """
        SELECT COUNT(1) FROM dbo.MarathonProgress
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
