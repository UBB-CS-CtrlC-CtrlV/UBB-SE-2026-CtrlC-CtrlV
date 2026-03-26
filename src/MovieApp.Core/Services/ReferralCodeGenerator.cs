using System;

namespace MovieApp.Core.Services;

/// <summary>
/// Generates referral codes for ambassador accounts.
/// </summary>
public sealed class ReferralCodeGenerator : IReferralCodeGenerator
{
    /// <summary>
    /// Builds a referral code from the supplied username and user identifier.
    /// </summary>
    public string Generate(string username, int userId)
    {
        var year = DateTime.UtcNow.Year;
        return $"{username.ToUpperInvariant()}{year}-{userId}";
    }
}
