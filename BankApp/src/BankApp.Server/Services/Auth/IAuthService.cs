using BankApp.Contracts.DTOs.Auth;
using BankApp.Contracts.Enums;
using ErrorOr;

namespace BankApp.Server.Services.Auth;

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
    /// <returns>
    /// <see cref="Result.Success"/> on success,
    /// or an error if the user does not exist.
    /// </returns>
    ErrorOr<Success> ResendOTP(int userId, string method);

    /// <summary>
    /// Initiates a password reset flow for the given email address.
    /// </summary>
    /// <param name="email">The email address of the user requesting a reset.</param>
    /// <returns>
    /// <see cref="Result.Success"/> on success,
    /// or an error if no account exists for the given email.
    /// </returns>
    ErrorOr<Success> RequestPasswordReset(string email);

    /// <summary>
    /// Resets the user's password using a valid reset token.
    /// </summary>
    /// <param name="token">The password reset token.</param>
    /// <param name="newPassword">The new plain-text password.</param>
    /// <returns>
    /// <see cref="ResetPasswordResult.Success"/> if the password was reset and all post-reset steps succeeded,
    /// <see cref="ResetPasswordResult.ExpiredToken"/> if the token has expired,
    /// <see cref="ResetPasswordResult.TokenAlreadyUsed"/> if the token was already consumed,
    /// <see cref="ResetPasswordResult.InvalidToken"/> if the token does not exist or the password update failed,
    /// or <see cref="ResetPasswordResult.Failed"/> if the password was updated but a post-reset security step
    /// (marking the token as used or invalidating active sessions) failed.
    /// </returns>
    ResetPasswordResult ResetPassword(string token, string newPassword);

    /// <summary>
    /// Logs out the user by invalidating the specified session token.
    /// </summary>
    /// <param name="token">The session token to invalidate.</param>
    /// <returns>
    /// <see cref="Result.Success"/> on success,
    /// or an error if no active session exists for the given token.
    /// </returns>
    ErrorOr<Success> Logout(string token);

    /// <summary>
    /// Validates a password reset token without consuming it.
    /// </summary>
    /// <param name="token">The reset token to verify.</param>
    /// <returns>A <see cref="ResetTokenValidationResult"/> indicating the token's validity.</returns>
    ResetTokenValidationResult VerifyResetToken(string token);
}