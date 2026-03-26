namespace MovieApp.Core.Repositories;

/// <summary>
/// Provides persistence operations for ambassador and referral workflows.
/// </summary>
public interface IAmbassadorRepository
{
    /// <summary>
    /// Checks whether a referral code exists.
    /// </summary>
    Task<bool> IsReferralCodeValidAsync(string referralCode, CancellationToken cancellationToken = default);
    /// <summary>
    /// Gets the ambassador code owned by the specified user.
    /// </summary>
    Task<string?> GetReferralCodeAsync(int userId, CancellationToken cancellationToken = default);
    /// <summary>
    /// Creates an ambassador profile for the specified user.
    /// </summary>
    Task CreateAmbassadorProfileAsync(int userId, string referralCode, CancellationToken cancellationToken = default);
    /// <summary>
    /// Resolves a referral code to the owning user identifier.
    /// </summary>
    Task<int?> GetUserIdByReferralCodeAsync(string referralCode, CancellationToken cancellationToken = default);
    /// <summary>
    /// Logs a referral interaction for a specific event.
    /// </summary>
    Task AddReferralLogAsync(int ambassadorId, int friendId, int eventId, CancellationToken cancellationToken = default);
    /// <summary>
    /// Evaluates whether a referral reward should be granted.
    /// </summary>
    Task<bool> TryApplyRewardAsync(int ambassadorId, CancellationToken cancellationToken = default);
    /// <summary>
    /// Gets referral history rows for the specified ambassador.
    /// </summary>
    Task<System.Collections.Generic.IEnumerable<MovieApp.Core.Models.ReferralHistoryItem>> GetReferralHistoryAsync(int ambassadorId, CancellationToken cancellationToken = default);
    /// <summary>
    /// Gets the remaining redeemable referral reward balance.
    /// </summary>
    Task<int> GetRewardBalanceAsync(int userId, CancellationToken cancellationToken = default);
    /// <summary>
    /// Consumes one referral reward from the specified user.
    /// </summary>
    Task DecrementRewardBalanceAsync(int userId, CancellationToken cancellationToken = default);
}
