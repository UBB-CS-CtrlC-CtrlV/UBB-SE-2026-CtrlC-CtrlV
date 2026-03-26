using Microsoft.Data.SqlClient;
using MovieApp.Core.Models;
using MovieApp.Core.Repositories;

namespace MovieApp.Infrastructure;

public sealed class SqlTriviaRewardRepository(DatabaseOptions databaseOptions) : ITriviaRewardRepository
{
    private readonly string _connectionString = databaseOptions.ConnectionString;

    public async Task AddAsync(TriviaReward reward, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(TriviaRewardSqlQueries.Insert, connection);
        command.Parameters.AddWithValue("@userId", reward.UserId);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<TriviaReward?> GetUnredeemedByUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(TriviaRewardSqlQueries.SelectUnredeemedByUser, connection);
        command.Parameters.AddWithValue("@userId", userId);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return MapTriviaReward(reader);
    }

    public async Task MarkAsRedeemedAsync(int rewardId, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(TriviaRewardSqlQueries.MarkAsRedeemed, connection);
        command.Parameters.AddWithValue("@rewardId", rewardId);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static TriviaReward MapTriviaReward(SqlDataReader reader)
    {
        return new TriviaReward
        {
            Id = reader.GetInt32(0),
            UserId = reader.GetInt32(1),
            IsRedeemed = reader.GetBoolean(2),
            CreatedAt = reader.GetDateTime(3)
        };
    }
}