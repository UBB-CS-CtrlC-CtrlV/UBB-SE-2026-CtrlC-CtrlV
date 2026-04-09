// <copyright file="AuthController.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>
using BankApp.Contracts.DTOs.Auth;
using BankApp.Contracts.Enums;
using BankApp.Server.Services.Auth;
using Microsoft.AspNetCore.Mvc;

namespace BankApp.Server.Controllers;

/// <summary>
/// Controller responsible for handling all authentication-related operations,
/// including login, registration, OTP verification, password reset, and OAuth.
/// All endpoints are accessible under the /api/auth route.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ApiControllerBase
{
    private readonly IAuthService authService;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthController"/> class.
    /// </summary>
    /// <param name="authService">The authentication service used to handle business logic.</param>
    public AuthController(IAuthService authService)
    {
        this.authService = authService;
    }

    /// <summary>
    /// Authenticates a user with their email and password.
    /// If 2FA is enabled, an OTP will be sent and must be verified separately.
    /// </summary>
    /// <param name="request">The login request containing email and password.</param>
    /// <returns>
    /// 200 OK with a <see cref="LoginResponse"/> on success,
    /// or 400 Bad Request if credentials are invalid.
    /// </returns>
    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        LoginResponse response = this.authService.Login(request);
        if (!response.Success)
        {
            return this.BadRequest(response);
        }

        return this.Ok(response);
    }

    /// <summary>
    /// Registers a new user account in the system.
    /// </summary>
    /// <param name="request">The registration request containing user details such as name, email, and password.</param>
    /// <returns>
    /// 200 OK with a <see cref="RegisterResponse"/> on success,
    /// or 400 Bad Request if registration fails (e.g. email already in use).
    /// </returns>
    [HttpPost("register")]
    public IActionResult Register([FromBody] RegisterRequest request)
    {
        RegisterResponse response = this.authService.Register(request);
        if (!response.Success)
        {
            return this.BadRequest(response);
        }

        return this.Ok(response);
    }

    /// <summary>
    /// Verifies a One-Time Password (OTP) as part of the two-factor authentication (2FA) flow.
    /// This should be called after a successful login when 2FA is required.
    /// </summary>
    /// <param name="request">The OTP verification request containing the user ID and OTP code.</param>
    /// <returns>
    /// 200 OK with a <see cref="LoginResponse"/> including a JWT token on success,
    /// or 400 Bad Request if the OTP is invalid or expired.
    /// </returns>
    [HttpPost("verify-otp")]
    public IActionResult VerifyOTP([FromBody] VerifyOTPRequest request)
    {
        LoginResponse response = this.authService.VerifyOTP(request);
        if (!response.Success)
        {
            return this.BadRequest(response);
        }

        return this.Ok(response);
    }

    /// <summary>
    /// Initiates the password reset flow by sending a reset link to the provided email address.
    /// For security purposes, the response is always generic regardless of whether the email exists,
    /// preventing user enumeration attacks.
    /// </summary>
    /// <param name="request">The forgot password request containing the user's email address.</param>
    /// <returns>
    /// 200 OK with a generic message in all cases where email is provided,
    /// or 400 Bad Request if the email field is empty.
    /// </returns>
    [HttpPost("forgot-password")]
    public IActionResult ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
        {
            return this.BadRequest(new { error = "Email is required." });
        }

        this.authService.RequestPasswordReset(request.Email);

        // Always return an OK response with a generic message ( prevent malicious operations )
        return this.Ok(new { message = "If an account with that email exists, a password reset link has been sent." });
    }

    /// <summary>
    /// Resets the user's password using a valid reset token received via email.
    /// The token must not be expired or already used, and the new password must meet strength requirements.
    /// </summary>
    /// <param name="request">The reset password request containing the reset token and new password.</param>
    /// <returns>
    /// 200 OK on successful password reset,
    /// 400 Bad Request if the token is invalid, expired, or already used,
    /// or if the new password does not meet strength requirements.
    /// </returns>
    [HttpPost("reset-password")]
    public IActionResult ResetPassword([FromBody] ResetPasswordRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Token) || string.IsNullOrWhiteSpace(request.NewPassword))
        {
            return this.BadRequest(new { error = "Token and new password are required." });
        }

        if (!Utilities.ValidationUtilities.IsStrongPassword(request.NewPassword))
        {
            return this.BadRequest(new { error = "Password must be at least 8 characters with uppercase, lowercase, a digit, and a special character." });
        }

        ResetPasswordResult result = this.authService.ResetPassword(request.Token, request.NewPassword);
        if (result != ResetPasswordResult.Success)
        {
            return result switch
            {
                ResetPasswordResult.ExpiredToken => this.BadRequest(new { error = "The reset token has expired.", errorCode = "token_expired" }),
                ResetPasswordResult.TokenAlreadyUsed => this.BadRequest(new { error = "The reset token has already been used.", errorCode = "token_already_used" }),
                _ => this.BadRequest(new { error = "The reset token is invalid.", errorCode = "token_invalid" }),
            };
        }

        return this.Ok(new { message = "Password reset successfully. You may now log in with your new password." });
    }

    /// <summary>
    /// Logs out the currently authenticated user by invalidating their session.
    /// Note: Full JWT invalidation is not yet implemented. The token is extracted from
    /// the Authorization header but is not blacklisted server-side.
    /// </summary>
    /// <param name="authorization">The Authorization header containing the Bearer JWT token.</param>
    /// <returns>
    /// 200 OK on successful logout,
    /// or 400 Bad Request if no token is provided or the session is invalid.
    /// </returns>
    [HttpPost("logout")]
    public IActionResult Logout([FromHeader(Name = "Authorization")] string authorization)
    {
        // Bogdan: this implementation is not enough, still need to invalidate JWT, but this is not on original diagram
        // can expand in the future.
        if (string.IsNullOrWhiteSpace(authorization) || !authorization.StartsWith("Bearer "))
        {
            return this.BadRequest(new { error = "No token provided." });
        }

        string token = authorization.Substring("Bearer ".Length);

        if (!this.authService.Logout(token))
        {
            return this.BadRequest(new { error = "Invalid session." });
        }

        return this.Ok(new { message = "Logged out successfully." });
    }

    /// <summary>
    /// Resends a new OTP code to the user via the specified delivery method.
    /// The response is always generic to prevent user enumeration.
    /// </summary>
    /// <param name="userId">The ID of the user requesting a new OTP.</param>
    /// <param name="method">The delivery method for the OTP (default is "email").</param>
    /// <returns>200 OK with a generic confirmation message.</returns>
    [HttpPost("resend-otp")]
    public IActionResult ResendOTP([FromQuery] int userId, [FromQuery] string method = "email")
    {
        this.authService.ResendOTP(userId, method);
        return this.Ok(new { message = "If the user exists, a new code has been sent." });
    }

    /// <summary>
    /// Authenticates a user via an external OAuth provider (e.g. Google, GitHub).
    /// If the user does not exist, a new account may be created automatically.
    /// </summary>
    /// <param name="request">The OAuth login request containing the provider name and provider token.</param>
    /// <returns>
    /// 200 OK with a <see cref="LoginResponse"/> including a JWT token on success,
    /// or 400 Bad Request if the provider or token is missing, or authentication fails.
    /// </returns>
    [HttpPost("oauth-login")]
    public async Task<IActionResult> OAuthLogin([FromBody] OAuthLoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Provider) || string.IsNullOrWhiteSpace(request.ProviderToken))
        {
            return this.BadRequest(new { error = "Provider and ProviderToken are required." });
        }

        LoginResponse response = await this.authService.OAuthLoginAsync(request);

        if (!response.Success)
        {
            return this.BadRequest(response);
        }

        return this.Ok(response);
    }

    /// <summary>
    /// Verifies whether a password reset token is valid before allowing the user to proceed
    /// to the reset password form. Checks for expiry and prior usage.
    /// </summary>
    /// <param name="request">The request containing the reset token to validate.</param>
    /// <returns>
    /// 200 OK if the token is valid,
    /// or 400 Bad Request with a specific error code if the token is expired, already used, or invalid.
    /// </returns>
    [HttpPost("verify-reset-token")]
    public IActionResult VerifyResetToken([FromBody] VerifyTokenDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Token))
        {
            return this.BadRequest(new { error = "Token is required." });
        }

        ResetTokenValidationResult result = this.authService.VerifyResetToken(request.Token);
        if (result != ResetTokenValidationResult.Valid)
        {
            return result switch
            {
                ResetTokenValidationResult.Expired => this.BadRequest(new { error = "The reset token has expired.", errorCode = "token_expired" }),
                ResetTokenValidationResult.AlreadyUsed => this.BadRequest(new { error = "The reset token has already been used.", errorCode = "token_already_used" }),
                _ => this.BadRequest(new { error = "The reset token is invalid.", errorCode = "token_invalid" }),
            };
        }

        return this.Ok(new { message = "Token is valid." });
    }
}