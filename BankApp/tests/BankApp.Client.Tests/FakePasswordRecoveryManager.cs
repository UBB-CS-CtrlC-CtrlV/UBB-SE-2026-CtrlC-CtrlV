using BankApp.Client.Enums;
using BankApp.Client.Utilities;

namespace BankApp.Client.Tests;

internal class FakePasswordRecoveryManager : IPasswordRecoveryManager
{
    public bool CanResendCode { get; set; } = true;
    public int SecondsUntilResendAllowed { get; set; } = 0;
    public ForgotPasswordState StateToReturn { get; set; } = ForgotPasswordState.EmailSent;
    public bool PasswordValid { get; set; } = true;

    public Task<ForgotPasswordState> RequestCodeAsync(string email) =>
        Task.FromResult(this.StateToReturn);

    public Task<ForgotPasswordState> VerifyTokenAsync(string token) =>
        Task.FromResult(this.StateToReturn);

    public Task<ForgotPasswordState> ResetPasswordAsync(string token, string newPassword) =>
        Task.FromResult(this.StateToReturn);

    public bool IsPasswordValid(string password) => this.PasswordValid;
}
