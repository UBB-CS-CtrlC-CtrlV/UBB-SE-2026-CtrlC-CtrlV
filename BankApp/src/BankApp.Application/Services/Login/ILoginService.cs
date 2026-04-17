// <copyright file="ILoginService.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

using BankApp.Application.DTOs.Auth;
using ErrorOr;

namespace BankApp.Application.Services.Login;

/// <summary>
/// Defines operations for user login, logout, OAuth login, and two-factor authentication.
/// </summary>
public interface ILoginService
{
    /// <summary>
    /// Authenticates a user with email and password.
    /// </summary>
    /// <param name="request">The login credentials.</param>
    /// <param name="metadata">Request-derived metadata to store with the created session.</param>
    /// <returns>
    /// A <see cref="FullLogin"/> if credentials are correct and 2FA is not required,
    /// a <see cref="RequiresTwoFactor"/> if 2FA is enabled and an OTP has been dispatched,
    /// a validation error with code <c>invalid_email</c> if the email format is invalid,
    /// an unauthorized error with code <c>invalid_credentials</c> if the email/password is wrong,
    /// or a forbidden error with code <c>account_locked</c> if the account is locked.
    /// </returns>
    ErrorOr<LoginSuccess> Login(LoginRequest request, SessionMetadata? metadata = null);

    /// <summary>
    /// Authenticates a user through an OAuth provider.
    /// </summary>
    /// <param name="request">The OAuth login details.</param>
    /// <param name="metadata">Request-derived metadata to store with the created session.</param>
    /// <returns>
    /// A task that resolves to a <see cref="FullLogin"/> or <see cref="RequiresTwoFactor"/> on success,
    /// a validation error with code <c>unsupported_provider</c> if the provider is not supported,
    /// a validation error with code <c>invalid_google_token</c> if the Google token is rejected,
    /// a forbidden error with code <c>account_locked</c> if the account is locked,
    /// or a failure error if user or link creation fails.
    /// </returns>
    Task<ErrorOr<LoginSuccess>> OAuthLoginAsync(OAuthLoginRequest request, SessionMetadata? metadata = null);

    /// <summary>
    /// Verifies a one-time password for two-factor authentication.
    /// </summary>
    /// <param name="request">The OTP verification details.</param>
    /// <param name="metadata">Request-derived metadata to store with the created session.</param>
    /// <returns>
    /// A <see cref="FullLogin"/> on success,
    /// a not-found error if the user does not exist,
    /// or an unauthorized error with code <c>invalid_otp</c> if the code is invalid or expired.
    /// </returns>
    ErrorOr<LoginSuccess> VerifyOTP(VerifyOTPRequest request, SessionMetadata? metadata = null);

    /// <summary>
    /// Resends a one-time password to the specified user.
    /// </summary>
    /// <param name="userId">The identifier of the user.</param>
    /// <param name="method">The delivery method (e.g., "email").</param>
    /// <returns>
    /// <see cref="Result.Success"/> on success,
    /// or an error if the user does not exist or OTP generation fails.
    /// </returns>
    ErrorOr<Success> ResendOTP(int userId, string method);

    /// <summary>
    /// Logs out the user by invalidating the specified session token.
    /// </summary>
    /// <param name="token">The session token to invalidate.</param>
    /// <returns>
    /// <see cref="Result.Success"/> on success,
    /// or an error if no active session exists for the given token.
    /// </returns>
    ErrorOr<Success> Logout(string token);
}
