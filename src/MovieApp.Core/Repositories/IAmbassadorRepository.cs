namespace MovieApp.Core.Repositories;

public interface IAmbassadorRepository
{
    Task<bool> IsReferralCodeValidAsync(string referralCode, CancellationToken cancellationToken = default);
    Task<string?> GetReferralCodeAsync(int userId, CancellationToken cancellationToken = default);
    Task CreateAmbassadorProfileAsync(int userId, string referralCode, CancellationToken cancellationToken = default);
    Task<int?> GetUserIdByReferralCodeAsync(string referralCode, CancellationToken cancellationToken = default);
}
