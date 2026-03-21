namespace MovieApp.Core.Models;

/// Referral interaction model that tracks valid referral code usage.
/// Records when a referred user joins an event using an ambassador's code.
public sealed class ReferralLog
{
    public required int LogId { get; init; }

    public required int AmbassadorId { get; init; }

    public required int ReferredUserId { get; init; }

    public required int EventId { get; init; }

    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}
