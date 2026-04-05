// <copyright file="ProfileController.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>
using System.Collections.Generic;
using BankApp.Core.DTOs.Profile;
using BankApp.Core.Entities;
using BankApp.Infrastructure.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BankApp.Server.Controllers
{
    /// <summary>
    /// Controller responsible for handling user profile-related operations.
    /// All endpoints are accessible under the /api/profile route.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ProfileController : ControllerBase
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
        /// 200 OK with a <see cref="GetProfileResponse"/> containing user details on success,
        /// or 404 Not Found if the user does not exist.
        /// </returns>
        [HttpGet]
        public IActionResult GetProfile()
        {
            int userId = this.GetAuthenticatedUserId();

            User? user = this.profileService.GetUserById(userId);
            if (user == null)
            {
                return this.NotFound(new GetProfileResponse(false, "User not found."));
            }

            return this.Ok(new GetProfileResponse(true, "Successfully retrieved profile information.", user));
        }

        /// <summary>
        /// Updates the personal information of the currently authenticated user.
        /// The user ID is always taken from the authentication context, not from the request body.
        /// </summary>
        /// <param name="request">The update profile request containing the new personal information.</param>
        /// <returns>
        /// 200 OK with an <see cref="UpdateProfileResponse"/> on success,
        /// or 400 Bad Request if the update fails.
        /// </returns>
        [HttpPut]
        public IActionResult UpdateProfile([FromBody] UpdateProfileRequest request)
        {
            int userId = this.GetAuthenticatedUserId();
            request.UserId = userId; // override whatever the client sent

            UpdateProfileResponse response = this.profileService.UpdatePersonalInfo(request);

            if (!response.Success)
            {
                return this.BadRequest(response);
            }

            return this.Ok(response);
        }

        /// <summary>
        /// Changes the password of the currently authenticated user.
        /// The user ID is always taken from the authentication context, not from the request body.
        /// </summary>
        /// <param name="request">The change password request containing the old and new passwords.</param>
        /// <returns>
        /// 200 OK with a <see cref="ChangePasswordResponse"/> on success,
        /// or 400 Bad Request if the password change fails.
        /// </returns>
        [HttpPut("password")]
        public IActionResult ChangePassword([FromBody] ChangePasswordRequest request)
        {
            int userId = this.GetAuthenticatedUserId();
            request.UserId = userId; // override whatever the client sent

            ChangePasswordResponse response = this.profileService.ChangePassword(request);

            if (!response.Success)
            {
                return this.BadRequest(response);
            }

            return this.Ok(response);
        }

        /// <summary>
        /// Retrieves all OAuth provider links associated with the currently authenticated user.
        /// </summary>
        /// <returns>
        /// 200 OK with a list of <see cref="OAuthLink"/> on success,
        /// or 404 Not Found if no OAuth links exist for the user.
        /// </returns>
        [HttpGet("oauthlinks")]
        public IActionResult GetOAuthLinks()
        {
            int userId = this.GetAuthenticatedUserId();

            List<OAuthLink> links = this.profileService.GetOAuthLinks(userId);

            if (links.Count == 0)
            {
                return this.NotFound(links);
            }

            return this.Ok(links);
        }

        /// <summary>
        /// Retrieves the notification preferences of the currently authenticated user.
        /// </summary>
        /// <returns>
        /// 200 OK with a list of <see cref="NotificationPreference"/> on success,
        /// or 404 Not Found if no preferences exist for the user.
        /// </returns>
        [HttpGet("notifications/preferences")]
        public IActionResult GetNotificationPreferences()
        {
            int userId = this.GetAuthenticatedUserId();

            List<NotificationPreference> prefs = this.profileService.GetNotificationPreferences(userId);

            if (prefs.Count == 0)
            {
                return this.NotFound(prefs);
            }

            return this.Ok(prefs);
        }

        /// <summary>
        /// Updates the notification preferences of the currently authenticated user.
        /// </summary>
        /// <param name="prefs">The list of updated notification preferences.</param>
        /// <returns>
        /// 200 OK on success,
        /// or 400 Bad Request if the update fails.
        /// </returns>
        [HttpPut("notifications/preferences")]
        public IActionResult UpdateNotificationPreferences([FromBody] List<NotificationPreference> prefs)
        {
            int userId = this.GetAuthenticatedUserId();

            bool success = this.profileService.UpdateNotificationPreferences(userId, prefs);

            if (!success)
            {
                return this.BadRequest(false);
            }

            return this.Ok(true);
        }

        /// <summary>
        /// Verifies whether the provided password matches the currently authenticated user's password.
        /// </summary>
        /// <param name="password">The plain text password to verify.</param>
        /// <returns>
        /// 200 OK if the password is correct,
        /// or 400 Bad Request if the password does not match.
        /// </returns>
        [HttpPost("verify-password")]
        public IActionResult VerifyPassword([FromBody] string password)
        {
            int userId = this.GetAuthenticatedUserId();

            bool success = this.profileService.VerifyPassword(userId, password);

            if (!success)
            {
                return this.BadRequest(false);
            }

            return this.Ok(true);
        }

        /// <summary>
        /// Enables two-factor authentication (2FA) for the currently authenticated user
        /// using the specified delivery method.
        /// </summary>
        /// <param name="request">The request containing the desired 2FA method (e.g. email, SMS).</param>
        /// <returns>
        /// 200 OK with a <see cref="Toggle2FAResponse"/> on success,
        /// or 400 Bad Request if enabling 2FA fails.
        /// </returns>
        [HttpPut("2fa/enable")]
        public IActionResult Enable2FA([FromBody] Enable2FARequest request)
        {
            int userId = this.GetAuthenticatedUserId();

            bool success = this.profileService.Enable2FA(userId, request.Method);

            if (!success)
            {
                return this.BadRequest(new Toggle2FAResponse { Success = false });
            }

            return this.Ok(new Toggle2FAResponse { Success = true });
        }

        /// <summary>
        /// Disables two-factor authentication (2FA) for the currently authenticated user.
        /// </summary>
        /// <returns>
        /// 200 OK with a <see cref="Toggle2FAResponse"/> on success,
        /// or 400 Bad Request if disabling 2FA fails.
        /// </returns>
        [HttpPut("2fa/disable")]
        public IActionResult Disable2FA()
        {
            int userId = this.GetAuthenticatedUserId();

            bool success = this.profileService.Disable2FA(userId);

            if (!success)
            {
                return this.BadRequest(new Toggle2FAResponse { Success = false });
            }

            return this.Ok(new Toggle2FAResponse { Success = true });
        }

        /// <summary>
        /// Extracts the authenticated user's ID from the HTTP context,
        /// set by the authentication middleware.
        /// </summary>
        /// <returns>The ID of the currently authenticated user.</returns>
        private int GetAuthenticatedUserId() => (int)this.HttpContext.Items["UserId"] !;
    }
}