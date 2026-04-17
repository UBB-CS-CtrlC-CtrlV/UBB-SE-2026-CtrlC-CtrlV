// <copyright file="ProfileService.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

using BankApp.Application.DataTransferObjects.Profile;
using BankApp.Domain.Entities;
using BankApp.Domain.Enums;
using BankApp.Application.Repositories.Interfaces;
using BankApp.Application.Services.Security;
using BankApp.Application.Utilities;
using ErrorOr;
using Microsoft.Extensions.Logging;

namespace BankApp.Application.Services.Profile;

/// <summary>
/// Provides user profile management operations including personal info, passwords, 2FA, OAuth, and notifications.
/// </summary>
public class ProfileService : IProfileService
{
    private const string GoogleOAuthProvider = "Google";

    private readonly IUserRepository userRepository;
    private readonly IHashService hashService;
    private readonly ILogger<ProfileService> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProfileService"/> class.
    /// </summary>
    /// <param name="userRepository">The user repository.</param>
    /// <param name="hashService">The password hashing service.</param>
    /// <param name="logger">The logger.</param>
    public ProfileService(IUserRepository userRepository, IHashService hashService, ILogger<ProfileService> logger)
    {
        this.userRepository = userRepository;
        this.hashService = hashService;
        this.logger = logger;
    }

    /// <inheritdoc />
    public ErrorOr<ProfileInfo> GetProfile(int userId)
    {
        ErrorOr<User> userResult = userRepository.FindById(userId);
        if (userResult.IsError)
        {
            logger.LogWarning("Profile fetch failed: user {UserId} not found.", userId);
            return userResult.FirstError;
        }

        return new ProfileInfo(userResult.Value);
    }

    /// <inheritdoc />
    public ErrorOr<Success> UpdatePersonalInfo(UpdateProfileRequest request)
    {
        if (request.UserId == null)
        {
            return Error.Validation(code: "user_id_required", description: "User ID is required.");
        }

        int userId = request.UserId.Value;

        ErrorOr<User> userResult = userRepository.FindById(userId);
        if (userResult.IsError)
        {
            logger.LogWarning("Profile update failed: user {UserId} not found.", userId);
            return userResult.FirstError;
        }

        User user = userResult.Value;

        if (request.FullName != null)
        {
            if (string.IsNullOrWhiteSpace(request.FullName))
            {
                return Error.Validation(code: "full_name_required", description: "Full name is required.");
            }

            user.FullName = request.FullName.Trim();
        }

        if (request.PhoneNumber != null)
        {
            if (!ValidationUtilities.IsValidPhoneNumber(request.PhoneNumber))
            {
                return Error.Validation(code: "invalid_phone", description: "Invalid phone number.");
            }

            user.PhoneNumber = request.PhoneNumber;
        }

        if (request.DateOfBirth != null)
        {
            user.DateOfBirth = request.DateOfBirth;
        }

        if (request.Address != null)
        {
            user.Address = request.Address.Trim();
        }

        if (request.Nationality != null)
        {
            user.Nationality = request.Nationality.Trim();
        }

        if (request.PreferredLanguage != null)
        {
            if (string.IsNullOrWhiteSpace(request.PreferredLanguage))
            {
                return Error.Validation(code: "preferred_language_required", description: "Preferred language is required.");
            }

            user.PreferredLanguage = request.PreferredLanguage.Trim();
        }

        if (userRepository.UpdateUser(user).IsError)
        {
            logger.LogError("Profile update failed for user {UserId}.", userId);
            return Error.Failure(code: "update_failed", description: "Could not update user.");
        }

        return Result.Success;
    }

    /// <inheritdoc />
    public ErrorOr<Success> ChangePassword(ChangePasswordRequest request)
    {
        ErrorOr<User> userResult = userRepository.FindById(request.UserId);
        if (userResult.IsError)
        {
            logger.LogWarning("Password change failed: user {UserId} not found.", request.UserId);
            return userResult.FirstError;
        }

        User user = userResult.Value;

        if (!ValidationUtilities.IsStrongPassword(request.NewPassword))
        {
            return Error.Validation(code: "weak_password", description: "Password must contain at least one digit, one uppercase and one special symbol.");
        }

        ErrorOr<bool> verifyResult = hashService.Verify(request.CurrentPassword, user.PasswordHash);
        if (verifyResult.IsError)
        {
            logger.LogError("Hash verification threw during password change for user {UserId}.", user.Id);
            return verifyResult.FirstError;
        }

        if (!verifyResult.Value)
        {
            logger.LogWarning("Password change failed for user {UserId}: incorrect current password.", user.Id);
            return Error.Validation(code: "incorrect_password", description: "Current password is incorrect. Please try again.");
        }

        ErrorOr<string> newHashResult = hashService.GetHash(request.NewPassword);
        if (newHashResult.IsError)
        {
            logger.LogError("Hash generation failed during password change for user {UserId}.", user.Id);
            return newHashResult.FirstError;
        }

        if (userRepository.UpdatePassword(user.Id, newHashResult.Value).IsError)
        {
            logger.LogError("Password update failed for user {UserId}.", user.Id);
            return Error.Failure(code: "update_failed", description: "Could not update password. Please try again.");
        }

        logger.LogInformation("Password changed successfully for user {UserId}.", user.Id);
        return Result.Success;
    }

    /// <inheritdoc />
    public ErrorOr<Success> Enable2FA(int userId, TwoFactorMethod method)           // To Do: Change to 2FA
    {
        ErrorOr<User> userResult = userRepository.FindById(userId);
        if (userResult.IsError)
        {
            logger.LogWarning("Enable 2FA failed: user {UserId} not found.", userId);
            return userResult.FirstError;
        }

        User user = userResult.Value;
        user.Is2FAEnabled = true;
        user.Preferred2FAMethod = method.ToString();

        if (userRepository.UpdateUser(user).IsError)
        {
            logger.LogError("Failed to enable 2FA for user {UserId}.", userId);
            return Error.Failure(code: "update_failed", description: "Failed to enable 2FA.");
        }

        logger.LogInformation("2FA enabled for user {UserId} via {Method}.", userId, method);
        return Result.Success;
    }

    /// <inheritdoc />
    public ErrorOr<Success> Disable2FA(int userId)
    {
        ErrorOr<User> userResult = userRepository.FindById(userId);
        if (userResult.IsError)
        {
            logger.LogWarning("Disable 2FA failed: user {UserId} not found.", userId);
            return userResult.FirstError;
        }

        User user = userResult.Value;
        user.Is2FAEnabled = false;
        user.Preferred2FAMethod = null;

        if (userRepository.UpdateUser(user).IsError)
        {
            logger.LogError("Failed to disable 2FA for user {UserId}.", userId);
            return Error.Failure(code: "update_failed", description: "Failed to disable 2FA.");
        }

        logger.LogInformation("2FA disabled for user {UserId}.", userId);
        return Result.Success;
    }

    /// <inheritdoc />
    public ErrorOr<List<OAuthLinkDataTransferObject>> GetOAuthLinks(int userId)
    {
        ErrorOr<User> userResult = userRepository.FindById(userId);
        if (userResult.IsError)
        {
            logger.LogWarning("OAuth links fetch failed: user {UserId} not found.", userId);
            return userResult.FirstError;
        }

        ErrorOr<List<OAuthLink>> linksResult = userRepository.GetLinkedProviders(userId);
        if (linksResult.IsError)
        {
            logger.LogError("Failed to fetch OAuth links for user {UserId}: {Error}", userId, linksResult.FirstError.Description);
            return linksResult.FirstError;
        }

        return linksResult.Value
            .Select(oauthLink => new OAuthLinkDataTransferObject
            {
                Id = oauthLink.Id,
                Provider = oauthLink.Provider,
                ProviderEmail = oauthLink.ProviderEmail,
                LinkedAt = oauthLink.LinkedAt,
            })
            .ToList();
    }

    /// <inheritdoc />
    public ErrorOr<Success> LinkOAuth(int userId, string provider)
    {
        if (!IsSupportedOAuthProvider(provider))
        {
            return Error.Validation(code: "unsupported_provider", description: "Only Google OAuth is supported.");
        }

        ErrorOr<User> userResult = userRepository.FindById(userId);
        if (userResult.IsError)
        {
            logger.LogWarning("OAuth link failed: user {UserId} not found.", userId);
            return userResult.FirstError;
        }

        ErrorOr<List<OAuthLink>> linksResult = userRepository.GetLinkedProviders(userId);
        if (linksResult.IsError)
        {
            logger.LogError("Failed to fetch OAuth links for user {UserId}: {Error}", userId, linksResult.FirstError.Description);
            return linksResult.FirstError;
        }

        if (linksResult.Value.Any(link => string.Equals(link.Provider, GoogleOAuthProvider, StringComparison.OrdinalIgnoreCase)))
        {
            return Error.Conflict(code: "oauth_already_linked", description: "Google OAuth is already linked.");
        }

        string providerUserId = $"local:{userId}:{GoogleOAuthProvider}";
        ErrorOr<Success> result = userRepository.SaveOAuthLink(userId, GoogleOAuthProvider, providerUserId, userResult.Value.Email);
        if (result.IsError)
        {
            logger.LogError("Failed to link Google OAuth for user {UserId}: {Error}", userId, result.FirstError.Description);
            return result.FirstError;
        }

        return Result.Success;
    }

    /// <inheritdoc />
    public ErrorOr<Success> UnlinkOAuth(int userId, string provider)
    {
        if (!IsSupportedOAuthProvider(provider))
        {
            return Error.Validation(code: "unsupported_provider", description: "Only Google OAuth is supported.");
        }

        ErrorOr<User> userResult = userRepository.FindById(userId);
        if (userResult.IsError)
        {
            logger.LogWarning("OAuth unlink failed: user {UserId} not found.", userId);
            return userResult.FirstError;
        }

        ErrorOr<List<OAuthLink>> linksResult = userRepository.GetLinkedProviders(userId);
        if (linksResult.IsError)
        {
            logger.LogError("Failed to fetch OAuth links for user {UserId}: {Error}", userId, linksResult.FirstError.Description);
            return linksResult.FirstError;
        }

        OAuthLink? link = linksResult.Value.FirstOrDefault(oauthLink =>
            string.Equals(oauthLink.Provider, GoogleOAuthProvider, StringComparison.OrdinalIgnoreCase));
        if (link is null)
        {
            return Error.NotFound(code: "oauth_link_not_found", description: "Google OAuth is not linked.");
        }

        ErrorOr<Success> result = userRepository.DeleteOAuthLink(link.Id);
        if (result.IsError)
        {
            logger.LogError("Failed to unlink Google OAuth for user {UserId}: {Error}", userId, result.FirstError.Description);
            return result.FirstError;
        }

        return Result.Success;
    }

    /// <inheritdoc />
    public ErrorOr<List<NotificationPreferenceDataTransferObject>> GetNotificationPreferences(int userId)
    {
        ErrorOr<User> userResult = userRepository.FindById(userId);
        if (userResult.IsError)
        {
            logger.LogWarning("Notification preferences fetch failed: user {UserId} not found.", userId);
            return userResult.FirstError;
        }

        ErrorOr<List<NotificationPreference>> prefsResult = userRepository.GetNotificationPreferences(userId);
        if (prefsResult.IsError)
        {
            logger.LogError("Failed to fetch notification preferences for user {UserId}: {Error}", userId, prefsResult.FirstError.Description);
            return prefsResult.FirstError;
        }

        return prefsResult.Value
            .Select(preference => new NotificationPreferenceDataTransferObject
            {
                Id = preference.Id,
                UserId = preference.UserId,
                Category = preference.Category,
                PushEnabled = preference.PushEnabled,
                EmailEnabled = preference.EmailEnabled,
                SmsEnabled = preference.SmsEnabled,
                MinAmountThreshold = preference.MinAmountThreshold,
            })
            .ToList();
    }

    /// <inheritdoc />
    public ErrorOr<Success> UpdateNotificationPreferences(int userId, List<NotificationPreferenceDataTransferObject> preferences)
    {
        ErrorOr<User> userResult = userRepository.FindById(userId);
        if (userResult.IsError)
        {
            logger.LogWarning("Notification preferences update failed: user {UserId} not found.", userId);
            return userResult.FirstError;
        }

        List<NotificationPreference> entities = preferences
            .Select(preference => new NotificationPreference
            {
                Id = preference.Id,
                UserId = preference.UserId,
                Category = preference.Category,
                PushEnabled = preference.PushEnabled,
                EmailEnabled = preference.EmailEnabled,
                SmsEnabled = preference.SmsEnabled,
                MinAmountThreshold = preference.MinAmountThreshold,
            })
            .ToList();

        if (userRepository.UpdateNotificationPreferences(userId, entities).IsError)
        {
            logger.LogError("Failed to update notification preferences for user {UserId}.", userId);
            return Error.Failure(code: "update_failed", description: "Failed to update notification preferences.");
        }

        return Result.Success;
    }

    /// <inheritdoc />
    public ErrorOr<bool> VerifyPassword(int userId, string password)
    {
        ErrorOr<User> userResult = userRepository.FindById(userId);
        if (userResult.IsError)
        {
            logger.LogWarning("Password verification failed: user {UserId} not found.", userId);
            return userResult.FirstError;
        }

        return hashService.Verify(password, userResult.Value.PasswordHash);
    }

    /// <inheritdoc />
    public ErrorOr<List<SessionDataTransferObject>> GetActiveSessions(int userId)
    {
        ErrorOr<User> userResult = userRepository.FindById(userId);
        if (userResult.IsError)
        {
            logger.LogWarning("Get sessions failed: user {UserId} not found.", userId);
            return userResult.FirstError;
        }

        ErrorOr<List<Session>> sessionsResult = userRepository.GetActiveSessions(userId);
        if (sessionsResult.IsError)
        {
            logger.LogError("Failed to fetch sessions for user {UserId}: {Error}", userId, sessionsResult.FirstError.Description);
            return sessionsResult.FirstError;
        }

        return sessionsResult.Value
            .Select(session => new SessionDataTransferObject
            {
                Id = session.Id,
                DeviceInfo = session.DeviceInfo,
                Browser = session.Browser,
                IpAddress = session.IpAddress,
                LastActiveAt = session.LastActiveAt,
                ExpiresAt = session.ExpiresAt,
                CreatedAt = session.CreatedAt,
            })
            .ToList();
    }

    /// <inheritdoc />
    public ErrorOr<Success> RevokeSession(int userId, int sessionId)
    {
        ErrorOr<User> userResult = userRepository.FindById(userId);
        if (userResult.IsError)
        {
            logger.LogWarning("Revoke session failed: user {UserId} not found.", userId);
            return userResult.FirstError;
        }

        ErrorOr<Success> result = userRepository.RevokeSession(userId, sessionId);
        if (result.IsError)
        {
            logger.LogError("Failed to revoke session {SessionId} for user {UserId}.", sessionId, userId);
            return result.FirstError;
        }

        logger.LogInformation("Session {SessionId} revoked for user {UserId}.", sessionId, userId);
        return Result.Success;
    }

    private static bool IsSupportedOAuthProvider(string provider) =>
        string.Equals(provider, GoogleOAuthProvider, StringComparison.OrdinalIgnoreCase);
}
