// <copyright file="AuthController.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

using BankApp.Application.DataTransferObjects;
using BankApp.Application.DataTransferObjects.Auth;
using BankApp.Application.Services.Login;
using BankApp.Application.Services.PasswordRecovery;
using BankApp.Application.Services.Registration;
using BankApp.Application.Utilities;
using ErrorOr;
using Microsoft.AspNetCore.Mvc;

namespace BankApp.Api.Controllers;

/// <summary>
/// Controller responsible for handling all authentication-related operations,
/// including login, registration, OTP verification, password reset, and OAuth.
/// All endpoints are accessible under the /api/auth route.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ApiControllerBase
{
    private const string BearerPrefix = "Bearer ";
    private const int DeviceInfoMaxLength = 255;
    private const int BrowserMaxLength = 100;
    private const int IpAddressMaxLength = 45;

    private readonly ILoginService loginService;
    private readonly IRegistrationService registrationService;
    private readonly IPasswordRecoveryService passwordRecoveryService;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthController"/> class.
    /// </summary>
    /// <param name="loginService">The login service used to handle login, logout, and 2FA operations.</param>
    /// <param name="registrationService">The registration service used to handle user registration.</param>
    /// <param name="passwordRecoveryService">The password recovery service used to handle password reset operations.</param>
    public AuthController(ILoginService loginService, IRegistrationService registrationService,
        IPasswordRecoveryService passwordRecoveryService)
    {
        this.loginService = loginService;
        this.registrationService = registrationService;
        this.passwordRecoveryService = passwordRecoveryService;
    }

    /// <summary>
    /// Authenticates a user with their email and password.
    /// If 2FA is enabled, an OTP will be sent and <see cref="LoginSuccessResponse.Requires2FA"/> will be <see langword="true"/>.
    /// </summary>
    /// <param name="request">The login request containing email and password.</param>
    /// <returns>
    /// 200 OK with a <see cref="LoginSuccessResponse"/> on success,
    /// 400 Bad Request for invalid email format,
    /// 401 Unauthorized for wrong credentials,
    /// or 403 Forbidden if the account is locked.
    /// </returns>
    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        ErrorOr<LoginSuccess> result = this.loginService.Login(request, this.GetSessionMetadata());
        return this.ToActionResult(result, this.MapLoginSuccess);
    }

    /// <summary>
    /// Registers a new user account in the system.
    /// </summary>
    /// <param name="request">The registration request containing user details.</param>
    /// <returns>
    /// 204 No Content on success,
    /// 400 Bad Request if validation fails,
    /// or 409 Conflict if the email is already registered.
    /// </returns>
    [HttpPost("register")]
    public IActionResult Register([FromBody] RegisterRequest request)
    {
        return this.ToActionResult(this.registrationService.Register(request));
    }

    /// <summary>
    /// Verifies a One-Time Password (OTP) as part of the two-factor authentication (2FA) flow.
    /// Should be called after a successful login when <see cref="LoginSuccessResponse.Requires2FA"/> is <see langword="true"/>.
    /// </summary>
    /// <param name="request">The OTP verification request containing the user ID and OTP code.</param>
    /// <returns>
    /// 200 OK with a <see cref="LoginSuccessResponse"/> including a JWT token on success,
    /// 401 Unauthorized if the OTP is invalid or expired,
    /// or 404 Not Found if the user does not exist.
    /// </returns>
    [HttpPost("verify-otp")]
    public IActionResult VerifyOTP([FromBody] VerifyOTPRequest request)
    {
        ErrorOr<LoginSuccess> result = this.loginService.VerifyOTP(request, this.GetSessionMetadata());
        return this.ToActionResult(result, this.MapLoginSuccess);
    }

    /// <summary>
    /// Initiates the password reset flow by sending a reset link to the provided email address.
    /// The response is always generic to prevent user enumeration attacks.
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
            return this.BadRequest(new ApplicationErrorResponse { Error = "Email is required." });
        }

        // Always return a generic response regardless of whether the email exists.
        _ = this.passwordRecoveryService.RequestPasswordReset(request.Email);
        return this.Ok(new { message = "If an account with that email exists, a password reset link has been sent." });
    }

    /// <summary>
    /// Resets the user's password using a valid reset token received via email.
    /// </summary>
    /// <param name="request">The reset password request containing the reset token and new password.</param>
    /// <returns>
    /// 204 No Content on successful password reset,
    /// 400 Bad Request if inputs are missing, the password is weak, or the token is invalid/expired/used,
    /// or 500 Internal Server Error if a post-reset cleanup step fails.
    /// </returns>
    [HttpPost("reset-password")]
    public IActionResult ResetPassword([FromBody] ResetPasswordRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Token) || string.IsNullOrWhiteSpace(request.NewPassword))
        {
            return this.BadRequest(new ApplicationErrorResponse { Error = "Token and new password are required." });
        }

        if (!ValidationUtilities.IsStrongPassword(request.NewPassword))
        {
            return this.BadRequest(new ApplicationErrorResponse
            {
                Error = "Password must be at least 8 characters with uppercase, lowercase, a digit, and a special character.",
                ErrorCode = "weak_password",
            });
        }

        return this.ToActionResult(this.passwordRecoveryService.ResetPassword(request.Token, request.NewPassword));
    }

    /// <summary>
    /// Logs out the currently authenticated user by invalidating their session.
    /// </summary>
    /// <param name="authorization">The Authorization header containing the Bearer JWT token.</param>
    /// <returns>
    /// 204 No Content on successful logout,
    /// or 400 Bad Request if no token is provided or the session is invalid.
    /// </returns>
    [HttpPost("logout")]
    public IActionResult Logout([FromHeader(Name = "Authorization")] string authorization)
    {
        if (string.IsNullOrWhiteSpace(authorization) || !authorization.StartsWith(BearerPrefix, StringComparison.Ordinal))
        {
            return this.BadRequest(new ApplicationErrorResponse { Error = "No token provided." });
        }

        string token = authorization.Substring(BearerPrefix.Length);
        return this.ToActionResult(this.loginService.Logout(token));
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
        // Always return a generic response regardless of outcome.
        _ = this.loginService.ResendOTP(userId, method);
        return this.Ok(new { message = "If the user exists, a new code has been sent." });
    }

    /// <summary>
    /// Authenticates a user via an external OAuth provider (e.g. Google).
    /// If the user does not exist, a new account is created automatically.
    /// </summary>
    /// <param name="request">The OAuth login request containing the provider name and provider token.</param>
    /// <returns>
    /// 200 OK with a <see cref="LoginSuccessResponse"/> on success,
    /// 400 Bad Request if the provider/token is missing or the provider is unsupported,
    /// or 403 Forbidden if the account is locked.
    /// </returns>
    [HttpPost("oauth-login")]
    public async Task<IActionResult> OAuthLogin([FromBody] OAuthLoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Provider) || string.IsNullOrWhiteSpace(request.ProviderToken))
        {
            return this.BadRequest(new ApplicationErrorResponse { Error = "Provider and ProviderToken are required." });
        }

        ErrorOr<LoginSuccess> result = await this.loginService.OAuthLoginAsync(request, this.GetSessionMetadata());
        return this.ToActionResult(result, this.MapLoginSuccess);
    }

    /// <summary>
    /// Verifies whether a password reset token is valid before allowing the user to proceed.
    /// </summary>
    /// <param name="request">The request containing the reset token to validate.</param>
    /// <returns>
    /// 204 No Content if the token is valid,
    /// or 400 Bad Request with a specific error code if the token is expired, already used, or invalid.
    /// </returns>
    [HttpPost("verify-reset-token")]
    public IActionResult VerifyResetToken([FromBody] VerifyTokenDataTransferObject request)
    {
        if (string.IsNullOrWhiteSpace(request.Token))
        {
            return this.BadRequest(new ApplicationErrorResponse { Error = "Token is required." });
        }

        return this.ToActionResult(this.passwordRecoveryService.VerifyResetToken(request.Token));
    }

    private IActionResult MapLoginSuccess(LoginSuccess success) => success switch
    {
        FullLogin full => this.Ok(new LoginSuccessResponse { UserId = full.UserId, Token = full.Token }),
        RequiresTwoFactor tfa => this.Ok(new LoginSuccessResponse { UserId = tfa.UserId, Requires2FA = true }),
        _ => this.StatusCode(StatusCodes.Status500InternalServerError, new ApplicationErrorResponse { Error = "Unexpected login result type." }),
    };

    private SessionMetadata GetSessionMetadata()
    {
        string? userAgent = TrimToMaxLength(this.Request.Headers["User-Agent"].ToString(), DeviceInfoMaxLength);

        return new SessionMetadata
        {
            DeviceInfo = userAgent,
            Browser = TrimToMaxLength(GetBrowserName(userAgent), BrowserMaxLength),
            IpAddress = TrimToMaxLength(GetClientIpAddress(this.HttpContext), IpAddressMaxLength),
        };
    }

    private static string? GetClientIpAddress(HttpContext context)
    {
        string forwardedFor = context.Request.Headers["X-Forwarded-For"].ToString();
        if (!string.IsNullOrWhiteSpace(forwardedFor))
        {
            return forwardedFor.Split(',').First().Trim();
        }

        return context.Connection.RemoteIpAddress?.ToString();
    }

    private static string? GetBrowserName(string? userAgent)
    {
        if (string.IsNullOrWhiteSpace(userAgent))
        {
            return null;
        }

        if (userAgent.Contains("Edg/", StringComparison.OrdinalIgnoreCase))
        {
            return "Microsoft Edge";
        }

        if (userAgent.Contains("Chrome/", StringComparison.OrdinalIgnoreCase))
        {
            return "Chrome";
        }

        if (userAgent.Contains("Firefox/", StringComparison.OrdinalIgnoreCase))
        {
            return "Firefox";
        }

        if (userAgent.Contains("Safari/", StringComparison.OrdinalIgnoreCase))
        {
            return "Safari";
        }

        return "Unknown Browser";
    }

    private static string? TrimToMaxLength(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Length <= maxLength ? value : value[..maxLength];
    }
}
