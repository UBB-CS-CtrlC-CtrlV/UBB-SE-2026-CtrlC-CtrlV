namespace BankApp.Infrastructure.Services.Infrastructure.Interfaces;

public interface IOtpService
{
    string GenerateTOTP(int userId);
    bool VerifyTOTP(int userId, string code);
    string GenerateSMSOTP(int userId);
    bool VerifySMSOTP(int userId, string code);
    bool IsExpired(DateTime expiredAt);
    void InvalidateOTP(int userId);

}
