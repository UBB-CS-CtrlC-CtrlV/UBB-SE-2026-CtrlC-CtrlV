// <copyright file="ProfileController.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

using BankApp.Contracts.DTOs;
using BankApp.Contracts.DTOs.Profile;
using BankApp.Contracts.Enums;
using BankApp.Server.Services.Profile;
using ErrorOr;
using Microsoft.AspNetCore.Mvc;

namespace BankApp.Server.Controllers;

/// <summary>
/// Controller responsible for handling user profile-related operations.
/// All endpoints are accessible under the /api/profile route.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ProfileController : ApiControllerBase
{
    private readonly IProfileService profileService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProfileController"/> class.
    /// </summary>
    /// <param name="profileService">The profile service used to handle business logic.</param>
    public ProfileController(IProfileService profileService)
    {
        this.profileService = profileService;
    }

    /// <summary>
    /// Retrieves the profile information of the currently authenticated user.
    /// </summary>
    /// <returns>
    /// 200 OK with a <see cref="ProfileInfo"/> on success,
    /// or 404 Not Found if the user does not exist.
    /// </returns>
    [HttpGet]
    public IActionResult GetProfile()
    {
        int userId = this.GetAuthenticatedUserId();
        return this.ToActionResult(this.profileService.GetProfile(userId), info => this.Ok(info));
    }

    /// <summary>
    /// Updates the personal information of the currently authenticated user.
    /// The user ID is always taken from the authentication context, not from the request body.
    /// </summary>
    /// <param name="request">The update profile request containing the new personal information.</param>
    /// <returns>
    /// 204 No Content on success,
    /// or 400/404 if the update fails.
    /// </returns>
    [HttpPut]
    public IActionResult UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        int userId = this.GetAuthenticatedUserId();
        request.UserId = userId;
        return this.ToActionResult(this.profileService.UpdatePersonalInfo(request));
    }

    /// <summary>
    /// Changes the password of the currently authenticated user.
    /// The user ID is always taken from the authentication context, not from the request body.
    /// </summary>
    /// <param name="request">The change password request containing the old and new passwords.</param>
    /// <returns>
    /// 204 No Content on success,
    /// or 400/404 if the password change fails.
    /// </returns>
    [HttpPut("password")]
    public IActionResult ChangePassword([FromBody] ChangePasswordRequest request)
    {
        int userId = this.GetAuthenticatedUserId();
        request.UserId = userId;
        return this.ToActionResult(this.profileService.ChangePassword(request));
    }

    /// <summary>
    /// Retrieves all OAuth provider links associated with the currently authenticated user.
    /// </summary>
    /// <returns>
    /// 200 OK with a list of <see cref="OAuthLinkDto"/> on success (may be empty),
    /// or 404 Not Found if the user does not exist.
    /// </returns>
    [HttpGet("oauth-links")]
    public IActionResult GetOAuthLinks()
    {
        int userId = this.GetAuthenticatedUserId();
        return this.ToActionResult(this.profileService.GetOAuthLinks(userId), links => this.Ok(links));
    }

    /// <summary>
    /// Retrieves the notification preferences of the currently authenticated user.
    /// </summary>
    /// <returns>
    /// 200 OK with a list of <see cref="NotificationPreferenceDto"/> on success (may be empty),
    /// or 404 Not Found if the user does not exist.
    /// </returns>
    [HttpGet("notifications/preferences")]
    public IActionResult GetNotificationPreferences()
    {
        int userId = this.GetAuthenticatedUserId();
        return this.ToActionResult(this.profileService.GetNotificationPreferences(userId), prefs => this.Ok(prefs));
    }

    /// <summary>
    /// Updates the notification preferences of the currently authenticated user.
    /// </summary>
    /// <param name="prefs">The list of updated notification preferences.</param>
    /// <returns>
    /// 204 No Content on success,
    /// or 400/404 if the update fails.
    /// </returns>
    [HttpPut("notifications/preferences")]
    public IActionResult UpdateNotificationPreferences([FromBody] List<NotificationPreferenceDto> prefs)
    {
        int userId = this.GetAuthenticatedUserId();
        return this.ToActionResult(this.profileService.UpdateNotificationPreferences(userId, prefs));
    }

    /// <summary>
    /// Verifies whether the provided password matches the currently authenticated user's password.
    /// </summary>
    /// <param name="password">The plain-text password to verify.</param>
    /// <returns>
    /// 200 OK with <see langword="true"/> if the password is correct,
    /// 200 OK with <see langword="false"/> if the password does not match,
    /// or 404/500 on unexpected errors.
    /// </returns>
    [HttpPost("verify-password")]
    public IActionResult VerifyPassword([FromBody] string password)
    {
        int userId = this.GetAuthenticatedUserId();
        ErrorOr<bool> result = this.profileService.VerifyPassword(userId, password);
        return this.ToActionResult(result, valid => this.Ok(valid));
    }

    /// <summary>
    /// Enables two-factor authentication (2FA) for the currently authenticated user
    /// using the specified delivery method.
    /// </summary>
    /// <param name="request">The request containing the desired 2FA method.</param>
    /// <returns>
    /// 204 No Content on success,
    /// or 400/404 if enabling 2FA fails.
    /// </returns>
    [HttpPut("2fa/enable")]
    public IActionResult Enable2FA([FromBody] Enable2FARequest request)
    {
        int userId = this.GetAuthenticatedUserId();
        return this.ToActionResult(this.profileService.Enable2FA(userId, request.Method));
    }

    /// <summary>
    /// Disables two-factor authentication (2FA) for the currently authenticated user.
    /// </summary>
    /// <returns>
    /// 204 No Content on success,
    /// or 400/404 if disabling 2FA fails.
    /// </returns>
    [HttpPut("2fa/disable")]
    public IActionResult Disable2FA()
    {
        int userId = this.GetAuthenticatedUserId();
        return this.ToActionResult(this.profileService.Disable2FA(userId));
    }
}
