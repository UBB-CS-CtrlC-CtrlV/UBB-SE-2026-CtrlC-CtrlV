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
    public GetProfileResponse? GetProfile(int userId)
    {
        ErrorOr<User> userResult = userRepository.FindById(userId);
        if (userResult.IsError)
        {
            logger.LogWarning("Profile fetch failed: user {UserId} not found.", userId);
            return null;
        }

        return new GetProfileResponse(true, "Successfully retrieved profile information.")
        {
            ProfileInfo = new ProfileInfo(userResult.Value),
        };
    }

    /// <inheritdoc />
    public UpdateProfileResponse UpdatePersonalInfo(UpdateProfileRequest request)
    {
        if (request.UserId == null)
        {
            return new UpdateProfileResponse(false, "Something went wrong. Please try again.");
        }

        int userId = request.UserId.Value;

        ErrorOr<User> userResult = userRepository.FindById(userId);
        if (userResult.IsError)
        {
            logger.LogWarning("Profile update failed: user {UserId} not found.", userId);
            return new UpdateProfileResponse(false, "User not found.");
        }

        User user = userResult.Value;

        if (request.PhoneNumber != null)
        {
            if (!ValidationUtilities.IsValidPhoneNumber(request.PhoneNumber))
            {
                return new UpdateProfileResponse(false, "Invalid phone number.");
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
            return new UpdateProfileResponse(false, "Could not update user.");
        }

        return new UpdateProfileResponse(true, "User profile updated successfully.");
    }

    /// <inheritdoc />
    public ChangePasswordResponse ChangePassword(ChangePasswordRequest request)
    {
        ErrorOr<User> userResult = userRepository.FindById(request.UserId);
        if (userResult.IsError)
        {
            logger.LogWarning("Password change failed: user {UserId} not found.", request.UserId);
            return new ChangePasswordResponse(false, "User not found.");
        }

        User user = userResult.Value;

        if (!ValidationUtilities.IsStrongPassword(request.NewPassword))
        {
            return new ChangePasswordResponse(false, "Password must contain at least one digit, one uppercase and one special symbol.");
        }

        if (!hashService.Verify(request.CurrentPassword, user.PasswordHash))
        {
            logger.LogWarning("Password change failed for user {UserId}: incorrect current password.", user.Id);
            return new ChangePasswordResponse(false, "Current password is incorrect. Please try again.");
        }

        if (userRepository.UpdatePassword(user.Id, hashService.GetHash(request.NewPassword)).IsError)
        {
            logger.LogError("Password update failed for user {UserId}.", user.Id);
            return new ChangePasswordResponse(false, "Could not update password. Please try again.");
        }

        logger.LogInformation("Password changed successfully for user {UserId}.", user.Id);
        return new ChangePasswordResponse(true, "Password changed successfully.");
    }

    /// <inheritdoc />
    public bool Enable2FA(int userId, TwoFactorMethod method)
    {
        ErrorOr<User> userResult = userRepository.FindById(userId);
        if (userResult.IsError)
        {
            logger.LogWarning("Enable 2FA failed: user {UserId} not found.", userId);
            return false;
        }

        User user = userResult.Value;
        user.Is2FAEnabled = true;
        user.Preferred2FAMethod = method.ToString();

        if (userRepository.UpdateUser(user).IsError)
        {
            logger.LogError("Failed to enable 2FA for user {UserId}.", userId);
            return false;
        }

        logger.LogInformation("2FA enabled for user {UserId} via {Method}.", userId, method);
        return true;
    }

    /// <inheritdoc />
    public bool Disable2FA(int userId)
    {
        ErrorOr<User> userResult = userRepository.FindById(userId);
        if (userResult.IsError)
        {
            logger.LogWarning("Disable 2FA failed: user {UserId} not found.", userId);
            return false;
        }

        User user = userResult.Value;
        user.Is2FAEnabled = false;
        user.Preferred2FAMethod = null;

        if (userRepository.UpdateUser(user).IsError)
        {
            logger.LogError("Failed to disable 2FA for user {UserId}.", userId);
            return false;
        }

        logger.LogInformation("2FA disabled for user {UserId}.", userId);
        return true;
    }

    /// <inheritdoc />
    public List<OAuthLinkDto> GetOAuthLinks(int userId)
    {
        ErrorOr<User> userResult = userRepository.FindById(userId);
        if (userResult.IsError)
        {
            logger.LogWarning("OAuth links fetch failed: user {UserId} not found.", userId);
            return new List<OAuthLinkDto>();
        }

        ErrorOr<List<OAuthLink>> linksResult = userRepository.GetLinkedProviders(userId);
        if (linksResult.IsError)
        {
            logger.LogError("Failed to fetch OAuth links for user {UserId}: {Error}", userId, linksResult.FirstError.Description);
            return new List<OAuthLinkDto>();
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
    public bool LinkOAuth(int userId, string provider)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public bool UnlinkOAuth(int userId, string provider)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public List<NotificationPreferenceDto> GetNotificationPreferences(int userId)
    {
        ErrorOr<User> userResult = userRepository.FindById(userId);
        if (userResult.IsError)
        {
            logger.LogWarning("Notification preferences fetch failed: user {UserId} not found.", userId);
            return new List<NotificationPreferenceDto>();
        }

        ErrorOr<List<NotificationPreference>> prefsResult = userRepository.GetNotificationPreferences(userId);
        if (prefsResult.IsError)
        {
            logger.LogError("Failed to fetch notification preferences for user {UserId}: {Error}", userId, prefsResult.FirstError.Description);
            return new List<NotificationPreferenceDto>();
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
    public bool UpdateNotificationPreferences(int userId, List<NotificationPreferenceDto> preferences)
    {
        ErrorOr<User> userResult = userRepository.FindById(userId);
        if (userResult.IsError)
        {
            logger.LogWarning("Notification preferences update failed: user {UserId} not found.", userId);
            return false;
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
            return false;
        }

        return true;
    }

    /// <inheritdoc />
    public bool VerifyPassword(int userId, string password)
    {
        ErrorOr<User> userResult = userRepository.FindById(userId);
        if (userResult.IsError)
        {
            logger.LogWarning("Password verification failed: user {UserId} not found.", userId);
            return false;
        }

        return hashService.Verify(password, userResult.Value.PasswordHash);
    }
}
