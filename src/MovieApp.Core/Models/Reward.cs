namespace MovieApp.Core.Models;

/// Reward model with type, owner, status, scope, and event-linked fields.
/// Supports same-event referral rewards via EventId + ApplicabilityScope.
public sealed class Reward
{
    public required int RewardId { get; init; }

    public required string RewardType { get; init; }

    public bool RedemptionStatus { get; set; }

    public string ApplicabilityScope { get; set; } = string.Empty;

    public double DiscountValue { get; set; }

    public required int OwnerUserId { get; init; }

    public int? EventId { get; set; }

    /// Marks the reward as redeemed.
    public void Redeem()
    {
        RedemptionStatus = true;
    }

    /// A reward is available when it has not yet been redeemed.
    public bool IsAvailable => !RedemptionStatus;
}
