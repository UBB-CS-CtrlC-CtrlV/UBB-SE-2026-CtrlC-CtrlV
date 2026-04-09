// <copyright file="IAuthService.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

using BankApp.Contracts.DTOs.Auth;
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
    /// <returns>
    /// A <see cref="FullLogin"/> if credentials are correct and 2FA is not required,
    /// a <see cref="RequiresTwoFactor"/> if 2FA is enabled and an OTP has been dispatched,
    /// a validation error with code <c>invalid_email</c> if the email format is invalid,
    /// an unauthorized error with code <c>invalid_credentials</c> if the email/password is wrong,
    /// or a forbidden error with code <c>account_locked</c> if the account is locked.
    /// </returns>
    ErrorOr<LoginSuccess> Login(LoginRequest request);

    /// <summary>
    /// Authenticates a user through an OAuth provider.
    /// </summary>
    /// <param name="request">The OAuth login details.</param>
    /// <returns>
    /// A task that resolves to a <see cref="FullLogin"/> or <see cref="RequiresTwoFactor"/> on success,
    /// a validation error with code <c>unsupported_provider</c> if the provider is not supported,
    /// a validation error with code <c>invalid_google_token</c> if the Google token is rejected,
    /// a forbidden error with code <c>account_locked</c> if the account is locked,
    /// or a failure error if user or link creation fails.
    /// </returns>
    Task<ErrorOr<LoginSuccess>> OAuthLoginAsync(OAuthLoginRequest request);

    /// <summary>
    /// Registers a new user with email and password.
    /// </summary>
    /// <param name="request">The registration details.</param>
    /// <returns>
    /// <see cref="Result.Success"/> on success,
    /// a validation error with code <c>invalid_email</c> if the email format is invalid,
    /// a validation error with code <c>weak_password</c> if the password does not meet strength requirements,
    /// a validation error with code <c>full_name_required</c> if the full name is empty,
    /// a conflict error with code <c>email_registered</c> if the email is already in use,
    /// or a failure error if user creation fails.
    /// </returns>
    ErrorOr<Success> Register(RegisterRequest request);

    /// <summary>
    /// Registers a new user through an OAuth provider.
    /// </summary>
    /// <param name="request">The OAuth registration details.</param>
    /// <returns>
    /// <see cref="Result.Success"/> on success,
    /// a validation error if the email is invalid,
    /// a conflict error if the OAuth account is already registered,
    /// or a failure error if user or link creation fails.
    /// </returns>
    ErrorOr<Success> OAuthRegister(OAuthRegisterRequest request);

    /// <summary>
    /// Verifies a one-time password for two-factor authentication.
    /// </summary>
    /// <param name="request">The OTP verification details.</param>
    /// <returns>
    /// A <see cref="FullLogin"/> on success,
    /// a not-found error if the user does not exist,
    /// or an unauthorized error with code <c>invalid_otp</c> if the code is invalid or expired.
    /// </returns>
    ErrorOr<LoginSuccess> VerifyOTP(VerifyOTPRequest request);

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
    /// Initiates a password reset flow for the given email address.
    /// </summary>
    /// <param name="email">The email address of the user requesting a reset.</param>
    /// <returns>
    /// <see cref="Result.Success"/> on success,
    /// or an error if no account exists for the given email or the token could not be saved.
    /// </returns>
    ErrorOr<Success> RequestPasswordReset(string email);

    /// <summary>
    /// Resets the user's password using a valid reset token.
    /// </summary>
    /// <param name="token">The password reset token.</param>
    /// <param name="newPassword">The new plain-text password.</param>
    /// <returns>
    /// <see cref="Result.Success"/> if the password was reset and all post-reset steps succeeded,
    /// a validation error with code <c>token_expired</c> if the token has expired,
    /// a validation error with code <c>token_already_used</c> if the token was already consumed,
    /// a validation error with code <c>token_invalid</c> if the token does not exist or the password update failed,
    /// or a failure error with code <c>reset_failed</c> if the password was updated but a post-reset
    /// security step (marking the token as used or invalidating active sessions) failed.
    /// </returns>
    ErrorOr<Success> ResetPassword(string token, string newPassword);

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
    /// <returns>
    /// <see cref="Result.Success"/> if the token is valid,
    /// a validation error with code <c>token_expired</c> if the token has expired,
    /// a validation error with code <c>token_already_used</c> if the token was already consumed,
    /// or a validation error with code <c>token_invalid</c> if the token does not exist.
    /// </returns>
    ErrorOr<Success> VerifyResetToken(string token);
}
