using Microsoft.Data.SqlClient;
using MovieApp.Core.Models;
using MovieApp.Core.Repositories;

namespace MovieApp.Infrastructure;

/// <summary>
/// SQL Server-backed repository for user slot-machine state. Uses the existing
/// dbo.UserSpins table and maps to <see cref="UserSpinData"/>.
/// </summary>
public sealed class SqlUserSlotMachineStateRepository : IUserSlotMachineStateRepository
{
    private readonly string _connectionString;

    public SqlUserSlotMachineStateRepository(DatabaseOptions databaseOptions)
    {
        ArgumentNullException.ThrowIfNull(databaseOptions);

        _connectionString = databaseOptions.ConnectionString;
    }

    public async Task<UserSpinData?> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT UserId, DailySpinsRemaining, BonusSpins, LastSlotSpinReset, LastTriviaSpinReset, LoginStreak, LastLoginDate, EventSpinRewardsToday FROM dbo.UserSpins WHERE UserId = @userId";

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@userId", userId);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return null;

        return new UserSpinData
        {
            UserId = reader.GetInt32(0),
            DailySpinsRemaining = reader.GetInt32(1),
            BonusSpins = reader.GetInt32(2),
            LastSlotSpinReset = reader.IsDBNull(3) ? default : reader.GetDateTime(3),
            LastTriviaSpinReset = reader.IsDBNull(4) ? default : reader.GetDateTime(4),
            LoginStreak = reader.GetInt32(5),
            LastLoginDate = reader.IsDBNull(6) ? default : reader.GetDateTime(6),
            EventSpinRewardsToday = reader.GetInt32(7)
        };
    }

    public async Task CreateAsync(UserSpinData state, CancellationToken cancellationToken = default)
    {
        const string sql = "INSERT INTO dbo.UserSpins (UserId, DailySpinsRemaining, BonusSpins, LastSlotSpinReset, LastTriviaSpinReset, LoginStreak, LastLoginDate, EventSpinRewardsToday) VALUES (@userId, @dailySpins, @bonusSpins, @lastSlotReset, @lastTriviaReset, @loginStreak, @lastLoginDate, @eventRewards)";

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@userId", state.UserId);
        command.Parameters.AddWithValue("@dailySpins", state.DailySpinsRemaining);
        command.Parameters.AddWithValue("@bonusSpins", state.BonusSpins);
        command.Parameters.AddWithValue("@lastSlotReset", (object?)state.LastSlotSpinReset ?? DBNull.Value);
        command.Parameters.AddWithValue("@lastTriviaReset", (object?)state.LastTriviaSpinReset ?? DBNull.Value);
        command.Parameters.AddWithValue("@loginStreak", state.LoginStreak);
        command.Parameters.AddWithValue("@lastLoginDate", (object?)state.LastLoginDate ?? DBNull.Value);
        command.Parameters.AddWithValue("@eventRewards", state.EventSpinRewardsToday);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task UpdateAsync(UserSpinData state, CancellationToken cancellationToken = default)
    {
        const string sql = "UPDATE dbo.UserSpins SET DailySpinsRemaining = @dailySpins, BonusSpins = @bonusSpins, LastSlotSpinReset = @lastSlotReset, LastTriviaSpinReset = @lastTriviaReset, LoginStreak = @loginStreak, LastLoginDate = @lastLoginDate, EventSpinRewardsToday = @eventRewards WHERE UserId = @userId";

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@dailySpins", state.DailySpinsRemaining);
        command.Parameters.AddWithValue("@bonusSpins", state.BonusSpins);
        command.Parameters.AddWithValue("@lastSlotReset", (object?)state.LastSlotSpinReset ?? DBNull.Value);
        command.Parameters.AddWithValue("@lastTriviaReset", (object?)state.LastTriviaSpinReset ?? DBNull.Value);
        command.Parameters.AddWithValue("@loginStreak", state.LoginStreak);
        command.Parameters.AddWithValue("@lastLoginDate", (object?)state.LastLoginDate ?? DBNull.Value);
        command.Parameters.AddWithValue("@eventRewards", state.EventSpinRewardsToday);
        command.Parameters.AddWithValue("@userId", state.UserId);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
