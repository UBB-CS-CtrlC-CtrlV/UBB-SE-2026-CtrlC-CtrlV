// <copyright file="ProfileController.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

using BankApp.Application.DataTransferObjects;
using BankApp.Application.DataTransferObjects.Profile;
using BankApp.Domain.Enums;
using BankApp.Application.Services.Profile;
using ErrorOr;
using Microsoft.AspNetCore.Mvc;

namespace BankApp.Api.Controllers;

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
    /// 200 OK with a list of <see cref="OAuthLinkDataTransferObject"/> on success (may be empty),
    /// or 404 Not Found if the user does not exist.
    /// </returns>
    [HttpGet("oauth-links")]
    public IActionResult GetOAuthLinks()
    {
        int userId = this.GetAuthenticatedUserId();
        return this.ToActionResult(this.profileService.GetOAuthLinks(userId), links => this.Ok(links));
    }

    /// <summary>
    /// Links a supported OAuth provider to the currently authenticated user.
    /// </summary>
    /// <param name="request">The provider link request.</param>
    /// <returns>204 No Content on success, or 400/404/409 if linking fails.</returns>
    [HttpPost("oauth/link")]
    public IActionResult LinkOAuth([FromBody] LinkOAuthRequest request)
    {
        int userId = this.GetAuthenticatedUserId();
        return this.ToActionResult(this.profileService.LinkOAuth(userId, request.Provider));
    }

    /// <summary>
    /// Unlinks a supported OAuth provider from the currently authenticated user.
    /// </summary>
    /// <param name="provider">The provider to unlink.</param>
    /// <returns>204 No Content on success, or 400/404 if unlinking fails.</returns>
    [HttpDelete("oauth/{provider}")]
    public IActionResult UnlinkOAuth(string provider)
    {
        int userId = this.GetAuthenticatedUserId();
        return this.ToActionResult(this.profileService.UnlinkOAuth(userId, provider));
    }

    /// <summary>
    /// Retrieves the notification preferences of the currently authenticated user.
    /// </summary>
    /// <returns>
    /// 200 OK with a list of <see cref="NotificationPreferenceDataTransferObject"/> on success (may be empty),
    /// or 404 Not Found if the user does not exist.
    /// </returns>
    [HttpGet("notifications/preferences")]
    public IActionResult GetNotificationPreferences()
    {
        int userId = this.GetAuthenticatedUserId();
        return this.ToActionResult(this.profileService.GetNotificationPreferences(userId), preferences => this.Ok(preferences));
    }

    /// <summary>
    /// Updates the notification preferences of the currently authenticated user.
    /// </summary>
    /// <param name="preferences">The list of updated notification preferences.</param>
    /// <returns>
    /// 204 No Content on success,
    /// or 400/404 if the update fails.
    /// </returns>
    [HttpPut("notifications/preferences")]
    public IActionResult UpdateNotificationPreferences([FromBody] List<NotificationPreferenceDataTransferObject> preferences)
    {
        int userId = this.GetAuthenticatedUserId();
        return this.ToActionResult(this.profileService.UpdateNotificationPreferences(userId, preferences));
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
    public IActionResult Enable2FA([FromBody] Enable2FactorAuthenticationRequest request)
    {
        int userId = this.GetAuthenticatedUserId();
        return this.ToActionResult(this.profileService.Enable2FactorAuthentication(userId, request.Method));
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
        return this.ToActionResult(this.profileService.Disable2FactorAuthentication(userId));
    }

    /// <summary>
    /// Retrieves all active sessions for the currently authenticated user.
    /// </summary>
    /// <returns>
    /// 200 OK with a list of active sessions on success,
    /// or 404 Not Found if the user does not exist.
    /// </returns>
    [HttpGet("sessions")]
    public IActionResult GetSessions()
    {
        int userId = this.GetAuthenticatedUserId();
        return this.ToActionResult(this.profileService.GetActiveSessions(userId), sessions => this.Ok(sessions));
    }

    /// <summary>
    /// Revokes a specific session for the currently authenticated user.
    /// </summary>
    /// <param name="sessionId">The identifier of the session to revoke.</param>
    /// <returns>
    /// 204 No Content on success,
    /// or 404/400 if the revocation fails.
    /// </returns>
    [HttpDelete("sessions/{sessionId}")]
    public IActionResult RevokeSession(int sessionId)
    {
        int userId = this.GetAuthenticatedUserId();
        return this.ToActionResult(this.profileService.RevokeSession(userId, sessionId));
    }
}
