// <copyright file="IPasswordRecoveryService.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

using ErrorOr;

namespace BankApp.Server.Services.PasswordRecovery;

/// <summary>
/// Defines operations for password reset flow, including requesting, verifying, and completing a password reset.
/// </summary>
public interface IPasswordRecoveryService
{
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
