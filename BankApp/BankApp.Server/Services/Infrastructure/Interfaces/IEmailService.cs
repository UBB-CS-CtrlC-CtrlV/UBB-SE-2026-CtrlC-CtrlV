namespace BankApp.Server.Services.Infrastructure.Interfaces;

public interface IEmailService
{
    void sendPasswordResetLink(string email, string token);
    void sendOTPCode(string email, string code);
    void SendLoginAlert(string email);
    void sendLockNotification(string email);
}