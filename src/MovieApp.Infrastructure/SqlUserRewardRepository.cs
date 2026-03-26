using Microsoft.Data.SqlClient;
using MovieApp.Core.Models;
using MovieApp.Core.Repositories;

namespace MovieApp.Infrastructure;

/// <summary>
/// SQL Server-backed repository for managing user discount rewards.
/// Handles access to the user_movie_discounts table.
/// </summary>
public sealed class SqlUserRewardRepository : IUserMovieDiscountRepository
{
    private readonly string _connectionString;

    public SqlUserRewardRepository(DatabaseOptions databaseOptions)
    {
        ArgumentNullException.ThrowIfNull(databaseOptions);
        _connectionString = databaseOptions.ConnectionString;
    }

    /// <summary>
    /// Creates a new discount reward record for a user.
    /// </summary>
    public async Task AddAsync(Reward reward, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            INSERT INTO dbo.UserMovieDiscounts (UserId, MovieId, DiscountPercentage)
            VALUES (@userId, @movieId, @discountPercentage)";

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@userId", reward.OwnerUserId);
        command.Parameters.AddWithValue("@movieId", reward.EventId ?? 0);
        command.Parameters.AddWithValue("@discountPercentage", (int)reward.DiscountValue);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    /// <summary>
    /// Retrieves all discount rewards associated with a user.
    /// </summary>
    public async Task<List<Reward>> GetDiscountsForUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT Id, UserId, MovieId, DiscountPercentage, CreatedAt
            FROM dbo.UserMovieDiscounts
            WHERE UserId = @userId
            ORDER BY CreatedAt DESC";

        var rewards = new List<Reward>();

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@userId", userId);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            rewards.Add(new Reward
            {
                RewardId = reader.GetInt32(0),
                OwnerUserId = reader.GetInt32(1),
                EventId = reader.IsDBNull(2) ? null : reader.GetInt32(2),
                DiscountValue = reader.IsDBNull(3) ? 0 : (double)reader.GetDecimal(3),
                RewardType = "MovieDiscount",
                RedemptionStatus = false,
                ApplicabilityScope = "MovieSpecific"
            });
        }

        return rewards;
    }
}
