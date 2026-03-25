namespace MovieApp.Core.Repositories;

public interface IAmbassadorRepository
{
    Task<bool> IsReferralCodeValidAsync(string referralCode, CancellationToken cancellationToken = default);
}
