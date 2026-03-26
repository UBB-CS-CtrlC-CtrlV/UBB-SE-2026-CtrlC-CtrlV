namespace MovieApp.Core.Models;

/// <summary>
/// Represents a redeemable trivia-wheel reward for a user.
/// </summary>
public sealed class TriviaReward
{
    /// <summary>
    /// Gets the reward identifier.
    /// </summary>
    public required int Id { get; init; }

    /// <summary>
    /// Gets the user that owns the reward.
    /// </summary>
    public required int UserId { get; init; }

    /// <summary>
    /// Gets or sets whether the reward has already been redeemed.
    /// </summary>
    public bool IsRedeemed { get; set; }

    /// <summary>
    /// Gets the creation timestamp of the reward.
    /// </summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// Marks the reward as redeemed.
    /// </summary>
    public void Redeem()
    {
        IsRedeemed = true;
    }

    /// <summary>
    /// Gets whether the reward can still be redeemed.
    /// </summary>
    public bool IsAvailable => !IsRedeemed;
}
