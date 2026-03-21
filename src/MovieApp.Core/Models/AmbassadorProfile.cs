namespace MovieApp.Core.Models;

/// Referral code model with stable code storage.
/// Each user can become an ambassador with a permanent, unique referral code.
public sealed class AmbassadorProfile
{
    public required int UserId { get; init; }

    public required string PermanentCode { get; init; }

    public int RewardBalance { get; set; }
}
