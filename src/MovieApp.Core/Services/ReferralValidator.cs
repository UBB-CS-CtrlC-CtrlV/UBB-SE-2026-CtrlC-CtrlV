using MovieApp.Core.Repositories;

namespace MovieApp.Core.Services;

/// <summary>
/// Validates referral-code usage rules during enrollment.
/// </summary>
public sealed class ReferralValidator : IReferralValidator
{
    private readonly IAmbassadorRepository _ambassadorRepository;

    /// <summary>
    /// Creates a validator backed by ambassador persistence.
    /// </summary>
    public ReferralValidator(IAmbassadorRepository ambassadorRepository)
    {
        _ambassadorRepository = ambassadorRepository;
    }

    /// <summary>
    /// Checks that a referral code exists and is not owned by the current user.
    /// </summary>
    public async Task<bool> IsValidReferralAsync(string referralCode, int currentUserId, CancellationToken cancellationToken = default)
    {
        var ownerId = await _ambassadorRepository.GetUserIdByReferralCodeAsync(referralCode, cancellationToken);
        if (ownerId is null)
        {
            return false; // Code does not exist
        }

        if (ownerId.Value == currentUserId)
        {
            return false; // Anti-Fraud: Cannot use your own code
        }

        return true;
    }
}
