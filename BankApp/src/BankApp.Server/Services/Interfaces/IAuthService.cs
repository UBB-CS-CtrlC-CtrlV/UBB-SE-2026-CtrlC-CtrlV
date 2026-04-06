using BankApp.Core.DTOs.Auth;
using BankApp.Core.Enums;
namespace BankApp.Infrastructure.Services.Interfaces
{
    /// <summary>
    /// Defines operations for user authentication, registration, and password management.
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// Authenticates a user with email and password.
        /// </summary>
        /// <param name="request">The login credentials.</param>
        /// <returns>A <see cref="LoginResponse"/> indicating the result of the login attempt.</returns>
        LoginResponse Login(LoginRequest request);

        /// <summary>
        /// Authenticates a user through an OAuth provider.
        /// </summary>
        /// <param name="request">The OAuth login details.</param>
        /// <returns>A task that resolves to a <see cref="LoginResponse"/> indicating the result.</returns>
        Task<LoginResponse> OAuthLoginAsync(OAuthLoginRequest request);

        /// <summary>
        /// Registers a new user with email and password.
        /// </summary>
        /// <param name="request">The registration details.</param>
        /// <returns>A <see cref="RegisterResponse"/> indicating the result.</returns>
        RegisterResponse Register(RegisterRequest request);

        /// <summary>
        /// Registers a new user through an OAuth provider.
        /// </summary>
        /// <param name="request">The OAuth registration details.</param>
        /// <returns>A <see cref="RegisterResponse"/> indicating the result.</returns>
        RegisterResponse OAuthRegister(OAuthRegisterRequest request);

        /// <summary>
        /// Verifies a one-time password for two-factor authentication.
        /// </summary>
        /// <param name="request">The OTP verification details.</param>
        /// <returns>A <see cref="LoginResponse"/> indicating the result.</returns>
        LoginResponse VerifyOTP(VerifyOTPRequest request);

        /// <summary>
        /// Resends a one-time password to the specified user.
        /// </summary>
        /// <param name="userId">The identifier of the user.</param>
        /// <param name="method">The delivery method (e.g., "email").</param>
        void ResendOTP(int userId, string method);

        /// <summary>
        /// Initiates a password reset flow for the given email address.
        /// </summary>
        /// <param name="email">The email address of the user requesting a reset.</param>
        void RequestPasswordReset(string email);

        /// <summary>
        /// Resets the user's password using a valid reset token.
        /// </summary>
        /// <param name="token">The password reset token.</param>
        /// <param name="newPasswordHash">The new password.</param>
        /// <returns>A <see cref="ResetPasswordResult"/> indicating the outcome.</returns>
        ResetPasswordResult ResetPassword(string token, string newPasswordHash);

        /// <summary>
        /// Logs out the user by invalidating the specified session token.
        /// </summary>
        /// <param name="token">The session token to invalidate.</param>
        /// <returns><see langword="true"/> if the session was successfully revoked; otherwise, <see langword="false"/>.</returns>
        bool Logout(string token);

        /// <summary>
        /// Validates a password reset token without consuming it.
        /// </summary>
        /// <param name="token">The reset token to verify.</param>
        /// <returns>A <see cref="ResetTokenValidationResult"/> indicating the token's validity.</returns>
        ResetTokenValidationResult VerifyResetToken(string token);
    }
}
