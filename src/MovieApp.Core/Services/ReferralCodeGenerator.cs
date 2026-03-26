using System.Text;
using System.Text.RegularExpressions;

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
        var normalizedUserToken = Regex.Replace(username.ToUpperInvariant(), "[^A-Z0-9]", string.Empty);
        var builder = new StringBuilder(normalizedUserToken.Length + 16);
        builder.Append(normalizedUserToken);
        builder.Append(year);
        builder.Append(userId);
        return builder.ToString();
    }
}
