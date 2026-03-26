using MovieApp.Core.Repositories;

namespace MovieApp.Core.Services;

/// <summary>
/// Records successful referral-code usage and triggers reward evaluation.
/// </summary>
public sealed class ReferralLogService : IReferralLogService
{
    private readonly IAmbassadorRepository _ambassadorRepository;

    /// <summary>
    /// Creates a logging service backed by ambassador persistence.
    /// </summary>
    public ReferralLogService(IAmbassadorRepository ambassadorRepository)
    {
        _ambassadorRepository = ambassadorRepository;
    }

    /// <summary>
    /// Stores a referral interaction when the supplied code resolves to an ambassador.
    /// </summary>
    public async Task LogReferralUsageAsync(string referralCode, int friendId, int eventId, CancellationToken cancellationToken = default)
    {
        var ambassadorId = await _ambassadorRepository.GetUserIdByReferralCodeAsync(referralCode, cancellationToken);
        if (ambassadorId.HasValue)
        {
            await _ambassadorRepository.AddReferralLogAsync(ambassadorId.Value, friendId, eventId, cancellationToken);
            await _ambassadorRepository.TryApplyRewardAsync(ambassadorId.Value, cancellationToken);
        }
    }
}
