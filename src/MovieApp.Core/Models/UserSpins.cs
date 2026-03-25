namespace MovieApp.Core.Models;

/// Stores user spin-related data such as daily spins, bonuses, and login tracking.
public sealed class UserSpinData
{
    public required int UserId { get; init; }

    public int DailySpinsRemaining { get; set; }

    public int BonusSpins { get; set; }

    public DateTime LastSlotSpinReset { get; set; }

    public DateTime LastTriviaSpinReset { get; set; }

    public int LoginStreak { get; set; }

    public DateTime LastLoginDate { get; set; }

    public int EventSpinRewardsToday { get; set; }

    /// Resets daily spins and event rewards (typically called once per day).
    public void ResetDailySpins(int defaultDailySpins)
    {
        DailySpinsRemaining = defaultDailySpins;
        EventSpinRewardsToday = 0;
        LastSlotSpinReset = DateTime.UtcNow;
    }

    /// Updates login streak based on last login date.
    public void UpdateLoginStreak()
    {
        var today = DateTime.UtcNow.Date;
        var lastLogin = LastLoginDate.Date;

        if (lastLogin == today.AddDays(-1))
        {
            LoginStreak++;
        }
        else if (lastLogin != today)
        {
            LoginStreak = 1;
        }

        LastLoginDate = today;
    }

    /// Indicates whether the user can spin (has spins available).
    public bool CanSpin => DailySpinsRemaining > 0 || BonusSpins > 0;
}