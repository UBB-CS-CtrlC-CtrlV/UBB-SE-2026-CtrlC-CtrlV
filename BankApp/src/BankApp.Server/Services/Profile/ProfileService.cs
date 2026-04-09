// <copyright file="ProfileService.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

using BankApp.Contracts.DTOs.Profile;
using BankApp.Contracts.Entities;
using BankApp.Contracts.Enums;
using BankApp.Server.Repositories.Interfaces;
using BankApp.Server.Services.Security;
using BankApp.Server.Utilities;
using ErrorOr;
using Microsoft.Extensions.Logging;

namespace BankApp.Server.Services.Profile;

/// <summary>
/// Provides user profile management operations including personal info, passwords, 2FA, OAuth, and notifications.
/// </summary>
public class ProfileService : IProfileService
{
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

        if (request.PhoneNumber != null)
        {
            if (!ValidationUtilities.IsValidPhoneNumber(request.PhoneNumber))
            {
                return Error.Validation(code: "invalid_phone", description: "Invalid phone number.");
            }

            user.PhoneNumber = request.PhoneNumber;
        }

        if (request.Address != null)
        {
            user.Address = request.Address;
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
    public ErrorOr<Success> Enable2FA(int userId, TwoFactorMethod method)
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
    public ErrorOr<List<OAuthLinkDto>> GetOAuthLinks(int userId)
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
            .Select(oauthLink => new OAuthLinkDto
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
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public ErrorOr<Success> UnlinkOAuth(int userId, string provider)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public ErrorOr<List<NotificationPreferenceDto>> GetNotificationPreferences(int userId)
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
            .Select(preference => new NotificationPreferenceDto
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
    public ErrorOr<Success> UpdateNotificationPreferences(int userId, List<NotificationPreferenceDto> preferences)
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
}
