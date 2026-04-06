using BankApp.Core.DTOs.Profile;
using BankApp.Core.Entities;
using BankApp.Core.Enums;
namespace BankApp.Infrastructure.Services.Interfaces
{
    /// <summary>
    /// Defines operations for user profile management, including personal info, passwords, 2FA, OAuth, and notifications.
    /// </summary>
    public interface IProfileService
    {
        /// <summary>
        /// Gets a user by their unique identifier.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <returns>The matching <see cref="User"/>, or <see langword="null"/> if not found.</returns>
        User? GetUserById(int userId);

        /// <summary>
        /// Updates the personal information for a user.
        /// </summary>
        /// <param name="request">The profile update details.</param>
        /// <returns>An <see cref="UpdateProfileResponse"/> indicating the result.</returns>
        UpdateProfileResponse UpdatePersonalInfo(UpdateProfileRequest request);

        /// <summary>
        /// Changes the password for a user.
        /// </summary>
        /// <param name="request">The password change details.</param>
        /// <returns>A <see cref="ChangePasswordResponse"/> indicating the result.</returns>
        ChangePasswordResponse ChangePassword(ChangePasswordRequest request);

        /// <summary>
        /// Enables two-factor authentication for the specified user.
        /// </summary>
        /// <param name="userId">The identifier of the user.</param>
        /// <param name="method">The preferred 2FA method.</param>
        /// <returns><see langword="true"/> if 2FA was enabled successfully; otherwise, <see langword="false"/>.</returns>
        bool Enable2FA(int userId, TwoFactorMethod method);

        /// <summary>
        /// Disables two-factor authentication for the specified user.
        /// </summary>
        /// <param name="userId">The identifier of the user.</param>
        /// <returns><see langword="true"/> if 2FA was disabled successfully; otherwise, <see langword="false"/>.</returns>
        bool Disable2FA(int userId);

        /// <summary>
        /// Gets all OAuth provider links for the specified user.
        /// </summary>
        /// <param name="userId">The identifier of the user.</param>
        /// <returns>A list of <see cref="OAuthLink"/> instances.</returns>
        List<OAuthLink> GetOAuthLinks(int userId);

        /// <summary>
        /// Links an OAuth provider to the specified user.
        /// </summary>
        /// <param name="userId">The identifier of the user.</param>
        /// <param name="provider">The OAuth provider name.</param>
        /// <returns><see langword="true"/> if the provider was linked successfully; otherwise, <see langword="false"/>.</returns>
        bool LinkOAuth(int userId, string provider);

        /// <summary>
        /// Unlinks an OAuth provider from the specified user.
        /// </summary>
        /// <param name="userId">The identifier of the user.</param>
        /// <param name="provider">The OAuth provider name.</param>
        /// <returns><see langword="true"/> if the provider was unlinked successfully; otherwise, <see langword="false"/>.</returns>
        bool UnlinkOAuth(int userId, string provider);

        /// <summary>
        /// Gets all notification preferences for the specified user.
        /// </summary>
        /// <param name="userId">The identifier of the user.</param>
        /// <returns>A list of <see cref="NotificationPreference"/> instances.</returns>
        List<NotificationPreference> GetNotificationPreferences(int userId);

        /// <summary>
        /// Replaces all notification preferences for the specified user.
        /// </summary>
        /// <param name="userId">The identifier of the user.</param>
        /// <param name="preferences">The updated list of notification preferences.</param>
        /// <returns><see langword="true"/> if the preferences were updated successfully; otherwise, <see langword="false"/>.</returns>
        bool UpdateNotificationPreferences(int userId, List<NotificationPreference> preferences);

        /// <summary>
        /// Verifies the password for the specified user.
        /// </summary>
        /// <param name="userId">The identifier of the user.</param>
        /// <param name="password">The plain-text password to verify.</param>
        /// <returns><see langword="true"/> if the password matches; otherwise, <see langword="false"/>.</returns>
        bool VerifyPassword(int userId, string password);

        /// <summary>
        /// Gets all active sessions for the specified user.
        /// </summary>
        /// <param name="userId">The identifier of the user.</param>
        /// <returns>A list of active <see cref="Session"/> instances.</returns>
        List<Session> GetActiveSessions(int userId);

        /// <summary>
        /// Revokes a specific session for the specified user.
        /// </summary>
        /// <param name="userId">The identifier of the user.</param>
        /// <param name="sessionId">The identifier of the session to revoke.</param>
        /// <returns><see langword="true"/> if revoked successfully; otherwise <see langword="false"/>.</returns>
        bool RevokeSession(int userId, int sessionId);
    }
}

