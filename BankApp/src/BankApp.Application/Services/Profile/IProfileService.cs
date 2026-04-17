// <copyright file="IProfileService.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

using BankApp.Application.DTOs.Profile;
using BankApp.Domain.Enums;
using ErrorOr;

namespace BankApp.Application.Services.Profile;

/// <summary>
/// Defines operations for managing user profiles, including personal info, passwords,
/// two-factor authentication, OAuth links, and notification preferences.
/// </summary>
public interface IProfileService
{
    /// <summary>
    /// Retrieves the profile information of the specified user.
    /// </summary>
    /// <param name="userId">The identifier of the user.</param>
    /// <returns>
    /// The user's <see cref="ProfileInfo"/> on success,
    /// or a not-found error if the user does not exist.
    /// </returns>
    ErrorOr<ProfileInfo> GetProfile(int userId);

    /// <summary>
    /// Updates the personal information (phone, address) of the specified user.
    /// </summary>
    /// <param name="request">The update request containing the user ID and fields to change.</param>
    /// <returns>
    /// <see cref="Result.Success"/> on success,
    /// a validation error with code <c>invalid_phone</c> if the phone number is malformed,
    /// a not-found error if the user does not exist,
    /// or a failure error if the database update fails.
    /// </returns>
    ErrorOr<Success> UpdatePersonalInfo(UpdateProfileRequest request);

    /// <summary>
    /// Changes the password of the specified user after verifying the current password.
    /// </summary>
    /// <param name="request">The change-password request.</param>
    /// <returns>
    /// <see cref="Result.Success"/> on success,
    /// a not-found error if the user does not exist,
    /// a validation error with code <c>weak_password</c> if the new password does not meet strength requirements,
    /// a validation error with code <c>incorrect_password</c> if the current password is wrong,
    /// or a failure error if the database update fails.
    /// </returns>
    ErrorOr<Success> ChangePassword(ChangePasswordRequest request);

    /// <summary>
    /// Enables two-factor authentication for the specified user.
    /// </summary>
    /// <param name="userId">The identifier of the user.</param>
    /// <param name="method">The two-factor delivery method to enable.</param>
    /// <returns>
    /// <see cref="Result.Success"/> on success,
    /// a not-found error if the user does not exist,
    /// or a failure error if the database update fails.
    /// </returns>
    ErrorOr<Success> Enable2FA(int userId, TwoFactorMethod method);

    /// <summary>
    /// Disables two-factor authentication for the specified user.
    /// </summary>
    /// <param name="userId">The identifier of the user.</param>
    /// <returns>
    /// <see cref="Result.Success"/> on success,
    /// a not-found error if the user does not exist,
    /// or a failure error if the database update fails.
    /// </returns>
    ErrorOr<Success> Disable2FA(int userId);

    /// <summary>
    /// Retrieves all OAuth provider links for the specified user.
    /// </summary>
    /// <param name="userId">The identifier of the user.</param>
    /// <returns>
    /// The list of <see cref="OAuthLinkDto"/> on success (may be empty),
    /// a not-found error if the user does not exist,
    /// or a failure error if the repository call fails.
    /// </returns>
    ErrorOr<List<OAuthLinkDto>> GetOAuthLinks(int userId);

    /// <summary>
    /// Links an OAuth provider account to the specified user. Not yet implemented.
    /// </summary>
    /// <param name="userId">The identifier of the user.</param>
    /// <param name="provider">The OAuth provider to link.</param>
    /// <returns>A failure error — not implemented.</returns>
    ErrorOr<Success> LinkOAuth(int userId, string provider);

    /// <summary>
    /// Unlinks an OAuth provider account from the specified user. Not yet implemented.
    /// </summary>
    /// <param name="userId">The identifier of the user.</param>
    /// <param name="provider">The OAuth provider to unlink.</param>
    /// <returns>A failure error — not implemented.</returns>
    ErrorOr<Success> UnlinkOAuth(int userId, string provider);

    /// <summary>
    /// Retrieves all notification preferences for the specified user.
    /// </summary>
    /// <param name="userId">The identifier of the user.</param>
    /// <returns>
    /// The list of <see cref="NotificationPreferenceDto"/> on success (may be empty),
    /// a not-found error if the user does not exist,
    /// or a failure error if the repository call fails.
    /// </returns>
    ErrorOr<List<NotificationPreferenceDto>> GetNotificationPreferences(int userId);

    /// <summary>
    /// Updates the notification preferences of the specified user.
    /// </summary>
    /// <param name="userId">The identifier of the user.</param>
    /// <param name="preferences">The updated preferences to persist.</param>
    /// <returns>
    /// <see cref="Result.Success"/> on success,
    /// a not-found error if the user does not exist,
    /// or a failure error if the database update fails.
    /// </returns>
    ErrorOr<Success> UpdateNotificationPreferences(int userId, List<NotificationPreferenceDto> preferences);

    /// <summary>
    /// Verifies whether the supplied plain-text password matches the user's stored hash.
    /// </summary>
    /// <param name="userId">The identifier of the user.</param>
    /// <param name="password">The plain-text password to verify.</param>
    /// <returns>
    /// <see langword="true"/> if the password matches,
    /// <see langword="false"/> if it does not,
    /// a not-found error if the user does not exist,
    /// or a failure error if the hash verification throws.
    /// </returns>
    ErrorOr<bool> VerifyPassword(int userId, string password);

    /// <summary>
    /// Gets all active sessions for the specified user.
    /// </summary>
    /// <param name="userId">The identifier of the user.</param>
    /// <returns>
    /// The list of active <see cref="SessionDto"/> on success,
    /// a not-found error if the user does not exist,
    /// or a failure error if the repository call fails.
    /// </returns>
    ErrorOr<List<SessionDto>> GetActiveSessions(int userId);

    /// <summary>
    /// Revokes a specific session for the specified user.
    /// </summary>
    /// <param name="userId">The identifier of the user.</param>
    /// <param name="sessionId">The identifier of the session to revoke.</param>
    /// <returns>
    /// <see cref="Result.Success"/> on success,
    /// a not-found error if the user does not exist,
    /// or a failure error if the revocation fails.
    /// </returns>
    ErrorOr<Success> RevokeSession(int userId, int sessionId);
}
