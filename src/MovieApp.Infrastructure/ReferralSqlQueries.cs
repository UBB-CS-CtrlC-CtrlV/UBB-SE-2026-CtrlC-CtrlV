namespace MovieApp.Infrastructure;

/// <summary>
/// Centralizes the referral-related queries.
/// </summary>
public static class ReferralSqlQueries
{
    /// <summary>
    /// Query to count how many unique users (friends) have used a specific referral code for a specific event.
    /// </summary>
    public const string CountUniqueFriendsForEvent = """
        SELECT COUNT(DISTINCT rl.FriendID)
        FROM dbo.ReferralLog rl
        JOIN dbo.AmbassadorProfile ap ON rl.AmbassadorID = ap.UserId
        WHERE ap.referral_code = @referralCode
          AND rl.EventID = @eventId;
        """;

    /// <summary>
    /// Checks if a referral code exists in the AmbassadorProfile table.
    /// </summary>
    public const string CheckReferralCodeExists = """
        SELECT CAST(CASE WHEN EXISTS (
            SELECT 1 FROM dbo.AmbassadorProfile WHERE referral_code = @referralCode
        ) THEN 1 ELSE 0 END AS BIT);
        """;

    /// <summary>
    /// Gets the referral code for a specific user ID.
    /// </summary>
    public const string SelectReferralCodeByUserId = """
        SELECT referral_code
        FROM dbo.AmbassadorProfile
        WHERE UserId = @userId;
        """;

    /// <summary>
    /// Inserts a newly generated referral code for an ambassador (user).
    /// </summary>
    public const string InsertAmbassadorProfile = """
        INSERT INTO dbo.AmbassadorProfile (UserId, referral_code)
        VALUES (@userId, @referralCode);
        """;

    /// <summary>
    /// Gets the User ID who owns the given referral code.
    /// </summary>
    public const string SelectUserIdByReferralCode = """
        SELECT UserId
        FROM dbo.AmbassadorProfile
        WHERE referral_code = @referralCode;
        """;
}
