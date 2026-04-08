using BankApp.Contracts.DTOs.Profile;
using BankApp.Contracts.Entities;
using BankApp.Contracts.Enums;
using BankApp.Server.Repositories.Interfaces;
using BankApp.Server.Services.Common.Interfaces;
using BankApp.Server.Services.Interfaces;
using BankApp.Server.Utilities;

namespace BankApp.Server.Services.Implementations;

/// <summary>
/// Provides user profile management operations including personal info, passwords, 2FA, OAuth, and notifications.
/// </summary>
public class ProfileService : IProfileService
{
    private readonly IUserRepository userRepository;
    private readonly IHashService hashService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProfileService"/> class.
    /// </summary>
    /// <param name="userRepository">The user repository.</param>
    /// <param name="hashService">The password hashing service.</param>
    public ProfileService(IUserRepository userRepository, IHashService hashService)
    {
        this.userRepository = userRepository;
        this.hashService = hashService;
    }

    /// <inheritdoc />
    public User? GetUserById(int userId)
    {
        return userRepository.FindById(userId);
    }

    /// <inheritdoc />
    public UpdateProfileResponse UpdatePersonalInfo(UpdateProfileRequest request)
    {
        if (request.UserId == null)
        {
            return new UpdateProfileResponse(false, "Something went wrong. Please try again.");
        }

        int userId = request.UserId.Value;

        User? user = userRepository.FindById(userId);
        if (user == null)
        {
            return new UpdateProfileResponse(false, "User not found.");
        }

        // Check and update phone number
        if (request.PhoneNumber != null)
        {
            if (!ValidationUtilities.IsValidPhoneNumber(request.PhoneNumber))
            {
                return new UpdateProfileResponse(false, "Invalid phone number.");
            }

            user.PhoneNumber = request.PhoneNumber;
        }

        // Check and update address
        if (request.Address != null)
        {
            user.Address = request.Address;
        }

        // Update the user in the repo
        if (userRepository.UpdateUser(user) == false)
        {
            return new UpdateProfileResponse(false, "Could not update user.");
        }

        return new UpdateProfileResponse(true, "User profile updated successfully.");
    }

    /// <inheritdoc />
    public ChangePasswordResponse ChangePassword(ChangePasswordRequest request)
    {
        User? user = userRepository.FindById(request.UserId);
        if (user == null)
        {
            return new ChangePasswordResponse(false, "User not found.");
        }

        if (!ValidationUtilities.IsStrongPassword(request.NewPassword))
        {
            return new ChangePasswordResponse(false, "Password must contain at least one digit, one uppercase and one special symbol.");
        }

        if (hashService.Verify(request.CurrentPassword, user.PasswordHash))
        {
            user.PasswordHash = hashService.GetHash(request.NewPassword);
            userRepository.UpdatePassword(user.Id, user.PasswordHash);
            return new ChangePasswordResponse(true, "Password changed successfully.");
        }
        else
        {
            return new ChangePasswordResponse(false, "Current password is incorrect. Please try again.");
        }
    }

    /// <inheritdoc />
    public bool Enable2FA(int userId, TwoFactorMethod method)
    {
        User? user = userRepository.FindById(userId);
        if (user == null)
        {
            return false;
        }
        user.Is2FAEnabled = true;
        user.Preferred2FAMethod = method.ToString();
        return userRepository.UpdateUser(user);
    }

    /// <inheritdoc />
    public bool Disable2FA(int userId)
    {
        User? user = userRepository.FindById(userId);
        if (user == null)
        {
            return false;
        }

        user.Is2FAEnabled = false;
        user.Preferred2FAMethod = null;
        return userRepository.UpdateUser(user);
    }

    /// <inheritdoc />
    public List<OAuthLink> GetOAuthLinks(int userId)
    {
        User? user = userRepository.FindById(userId);
        if (user == null)
        {
            return new List<OAuthLink>();
        }

        return userRepository.GetLinkedProviders(userId);
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
    public List<NotificationPreference> GetNotificationPreferences(int userId)
    {
        User? user = userRepository.FindById(userId);
        if (user == null)
        {
            return new List<NotificationPreference>();
        }

        return userRepository.GetNotificationPreferences(userId);
    }

    /// <inheritdoc />
    public bool UpdateNotificationPreferences(int userId, List<NotificationPreference> preferences)
    {
        User? user = userRepository.FindById(userId);
        if (user == null)
        {
            return false;
        }

        return userRepository.UpdateNotificationPreferences(userId, preferences);
    }

    /// <inheritdoc />
    public bool VerifyPassword(int userId, string password)
    {
        User? user = userRepository.FindById(userId);

        if (user == null)
        {
            return false;
        }

        return hashService.Verify(password, user.PasswordHash);
    }
}