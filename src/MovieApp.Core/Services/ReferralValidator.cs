using MovieApp.Core.Repositories;

namespace MovieApp.Core.Services;

public sealed class ReferralValidator : IReferralValidator
{
    private readonly IAmbassadorRepository _ambassadorRepository;

    public ReferralValidator(IAmbassadorRepository ambassadorRepository)
    {
        _ambassadorRepository = ambassadorRepository;
    }

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
