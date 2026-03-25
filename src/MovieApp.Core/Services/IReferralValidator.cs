namespace MovieApp.Core.Services;

public interface IReferralValidator
{
    Task<bool> IsValidReferralAsync(string referralCode, int currentUserId, CancellationToken cancellationToken = default);
}
