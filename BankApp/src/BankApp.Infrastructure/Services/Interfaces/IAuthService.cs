using BankApp.Core.DTOs.Auth;
using BankApp.Core.Enums;
namespace BankApp.Infrastructure.Services.Interfaces
{
    public interface IAuthService
    {
        LoginResponse Login(LoginRequest request);
        Task<LoginResponse> OAuthLoginAsync(OAuthLoginRequest request);
        RegisterResponse Register(RegisterRequest request);
        RegisterResponse OAuthRegister(OAuthRegisterRequest request);
        LoginResponse VerifyOTP(VerifyOTPRequest request);
        void ResendOTP(int userId, string method);
        void RequestPasswordReset(string email);
        ResetPasswordResult ResetPassword(string token, string newPasswordHash);
        bool Logout(string token);
        ResetTokenValidationResult VerifyResetToken(string token);
    }
}
