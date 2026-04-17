// <copyright file="LoginService.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

using BankApp.Application.DataTransferObjects.Auth;
using BankApp.Domain.Entities;
using BankApp.Domain.Enums;
using BankApp.Application.Repositories.Interfaces;
using BankApp.Application.Services.Notifications;
using BankApp.Application.Services.Security;
using BankApp.Application.Utilities;
using ErrorOr;
using Google.Apis.Auth;
using Microsoft.Extensions.Logging;

namespace BankApp.Application.Services.Login;

/// <summary>
/// Provides login, logout, OAuth login, and two-factor authentication operations.
/// </summary>
public class LoginService : ILoginService
{
    private static readonly Dictionary<int, int> FailedOtpAttempts = new Dictionary<int, int>();
    private readonly IAuthRepository authRepository;
    private readonly IHashService hashService;
    private readonly IJwtService jwtService;
    private readonly IOtpService otpService;
    private readonly IEmailService emailService;
    private readonly ILogger<LoginService> logger;

    private const int MaxFailedAttempts = 5;
    private const int LockoutMinutes = 15;
    private const int MaxFailedOtpAttempts = 3;
    private const int FailedLoginAttemptIncrement = 1;
    private const string GoogleOAuthProvider = "Google";
    private const string DefaultLanguage = "en";
    private const string TemporaryPasswordSuffix = "A1a!";
    private const string EmailTwoFactorMethod = nameof(TwoFactorMethod.Email);

    /// <summary>
    /// Initializes a new instance of the <see cref="LoginService"/> class.
    /// </summary>
    /// <param name="authRepository">The authentication repository.</param>
    /// <param name="hashService">The password hashing service.</param>
    /// <param name="jwtService">The JWT token service.</param>
    /// <param name="otpService">The one-time password service.</param>
    /// <param name="emailService">The email delivery service.</param>
    /// <param name="logger">The logger.</param>
    public LoginService(IAuthRepository authRepository, IHashService hashService, IJwtService jwtService,
        IOtpService otpService, IEmailService emailService, ILogger<LoginService> logger)
    {
        this.authRepository = authRepository;
        this.hashService = hashService;
        this.jwtService = jwtService;
        this.otpService = otpService;
        this.emailService = emailService;
        this.logger = logger;
    }

    /// <inheritdoc />
    public ErrorOr<LoginSuccess> Login(LoginRequest request, SessionMetadata? metadata = null)
    {
        if (!ValidationUtilities.IsValidEmail(request.Email))
        {
            return Error.Validation(code: "invalid_email", description: "Invalid email format.");
        }

        ErrorOr<User> userResult = authRepository.FindUserByEmail(request.Email);
        if (userResult.IsError)
        {
            logger.LogWarning("Login failed: user not found for email.");
            return Error.Unauthorized(code: "invalid_credentials", description: "Invalid email or password.");
        }

        User user = userResult.Value;

        Error? lockError = CheckAccountLock(user);
        if (lockError is not null)
        {
            return lockError.Value;
        }

        ErrorOr<bool> verifyResult = hashService.Verify(request.Password, user.PasswordHash);
        if (verifyResult.IsError)
        {
            logger.LogError("Password hash verification threw for user {UserId}: {Error}", user.Id, verifyResult.FirstError.Description);
            return verifyResult.FirstError;
        }

        if (!verifyResult.Value)
        {
            return HandleFailedPassword(user);
        }

        return user.Is2FAEnabled ? Handle2FA(user) : CompleteLogin(user, metadata);
    }

    /// <inheritdoc />
    public async Task<ErrorOr<LoginSuccess>> OAuthLoginAsync(OAuthLoginRequest request, SessionMetadata? metadata = null)
    {
        if (!request.Provider.Equals(GoogleOAuthProvider, StringComparison.OrdinalIgnoreCase))
        {
            return Error.Validation(code: "unsupported_provider", description: "Unsupported OAuth Provider.");
        }

        GoogleJsonWebSignature.Payload payload;
        try
        {
            payload = await GoogleJsonWebSignature.ValidateAsync(request.ProviderToken);
        }
        catch (InvalidJwtException)
        {
            logger.LogWarning("OAuth login rejected: invalid Google token.");
            return Error.Validation(code: "invalid_google_token", description: "Invalid Google authentication token.");
        }

        string providerUserId = payload.Subject;
        string email = payload.Email;
        string fullName = payload.Name;

        ErrorOr<OAuthLink> linkResult = authRepository.FindOAuthLink(request.Provider, providerUserId);
        User? user = null;

        if (!linkResult.IsError)
        {
            ErrorOr<User> userResult = authRepository.FindUserById(linkResult.Value.UserId);
            if (!userResult.IsError)
            {
                user = userResult.Value;
            }
        }

        if (user is null)
        {
            ErrorOr<User> byEmailResult = authRepository.FindUserByEmail(email);
            if (!byEmailResult.IsError)
            {
                user = byEmailResult.Value;
            }
            else
            {
                string generatedTemporaryPassword = Guid.NewGuid().ToString() + TemporaryPasswordSuffix;
                ErrorOr<string> hashResult = hashService.GetHash(generatedTemporaryPassword);
                if (hashResult.IsError)
                {
                    logger.LogError("Hash generation failed during OAuth user creation for provider {Provider}.", request.Provider);
                    return hashResult.FirstError;
                }

                User newUser = new User
                {
                    Email = email,
                    PasswordHash = hashResult.Value,
                    FullName = fullName,
                    PreferredLanguage = DefaultLanguage,
                    Is2FAEnabled = false,
                    IsLocked = false,
                    FailedLoginAttempts = default,
                };

                if (authRepository.CreateUser(newUser).IsError)
                {
                    logger.LogError("OAuth user creation failed for provider {Provider}.", request.Provider);
                    return Error.Failure(code: "user_creation_failed", description: "Failed to create user account.");
                }

                ErrorOr<User> createdResult = authRepository.FindUserByEmail(email);
                if (createdResult.IsError)
                {
                    logger.LogError("Failed to retrieve user after OAuth creation for provider {Provider}.", request.Provider);
                    return Error.Failure(code: "user_retrieval_failed", description: "Failed to retrieve created user.");
                }

                user = createdResult.Value;
            }

            OAuthLink newLink = new OAuthLink
            {
                UserId = user.Id,
                Provider = request.Provider,
                ProviderUserId = providerUserId,
                ProviderEmail = email,
            };

            if (authRepository.CreateOAuthLink(newLink).IsError)
            {
                logger.LogError("Failed to create OAuth link for user {UserId}, provider {Provider}.", user.Id, request.Provider);
                return Error.Failure(code: "oauth_link_failed", description: "Failed to link OAuth account.");
            }
        }

        Error? lockError = CheckAccountLock(user);
        if (lockError is not null)
        {
            return lockError.Value;
        }

        return user.Is2FAEnabled ? Handle2FA(user) : CompleteLogin(user, metadata);
    }

    /// <inheritdoc />
    public ErrorOr<LoginSuccess> VerifyOTP(VerifyOTPRequest request, SessionMetadata? metadata = null)
    {
        ErrorOr<User> userResult = authRepository.FindUserById(request.UserId);
        if (userResult.IsError)
        {
            logger.LogWarning("OTP verification failed: user {UserId} not found.", request.UserId);
            return Error.NotFound(code: "user_not_found", description: "User not found.");
        }

        User user = userResult.Value;

        ErrorOr<bool> verifyResult = VerifyOtpForPreferredMethod(user, request.OTPCode);
        if (verifyResult.IsError)
        {
            logger.LogError("OTP verification threw for user {UserId}: {Error}", user.Id, verifyResult.FirstError.Description);
            return verifyResult.FirstError;
        }

        if (!verifyResult.Value)
        {
            logger.LogWarning("OTP verification failed for user {UserId}: invalid or expired code.", user.Id);
            if (TrackFailedOtpAttempt(user.Id) >= MaxFailedOtpAttempts)
            {
                otpService.InvalidateOTP(user.Id);
                FailedOtpAttempts.Remove(user.Id);
                logger.LogWarning("OTP challenge invalidated for user {UserId} after {MaxAttempts} failed attempts.", user.Id, MaxFailedOtpAttempts);
                return Error.Unauthorized(code: "otp_attempts_exceeded", description: "Too many incorrect OTP entries. Please restart login.");
            }

            return Error.Unauthorized(code: "invalid_otp", description: "Invalid or expired OTP code.");
        }

        FailedOtpAttempts.Remove(user.Id);
        otpService.InvalidateOTP(user.Id);
        return CompleteLogin(user, metadata);
    }

    /// <inheritdoc />
    public ErrorOr<Success> ResendOTP(int userId, string method)
    {
        ErrorOr<User> userResult = authRepository.FindUserById(userId);
        if (userResult.IsError)
        {
            logger.LogWarning("OTP resend failed: user {UserId} not found.", userId);
            return userResult.FirstError;
        }

        User user = userResult.Value;

        ErrorOr<string> otpResult = GenerateOtpForMethod(user);
        if (otpResult.IsError)
        {
            logger.LogError("OTP generation failed during resend for user {UserId}: {Error}", user.Id, otpResult.FirstError.Description);
            return otpResult.FirstError;
        }

        if (string.Equals(method, EmailTwoFactorMethod, StringComparison.OrdinalIgnoreCase)
            || string.Equals(user.Preferred2FAMethod, EmailTwoFactorMethod, StringComparison.OrdinalIgnoreCase))
        {
            emailService.SendOTPCode(user.Email, otpResult.Value);
        }

        FailedOtpAttempts.Remove(user.Id);
        return Result.Success;
    }

    /// <inheritdoc />
    public ErrorOr<Success> Logout(string token)
    {
        ErrorOr<Session> sessionResult = authRepository.FindSessionByToken(token);
        if (sessionResult.IsError)
        {
            logger.LogWarning("Logout failed: session not found.");
            return sessionResult.FirstError;
        }

        _ = authRepository.UpdateSessionToken(sessionResult.Value.Id);
        logger.LogInformation("User {UserId} logged out.", sessionResult.Value.UserId);
        return Result.Success;
    }

    private Error? CheckAccountLock(User user)
    {
        if (!user.IsLocked)
        {
            return null;
        }

        if (user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTime.UtcNow)
        {
            logger.LogWarning("Login blocked: account {UserId} is locked until {LockoutEnd}.", user.Id, user.LockoutEnd);
            return Error.Forbidden(code: "account_locked", description: "Account is locked. Try again later.");
        }

        _ = authRepository.ResetFailedAttempts(user.Id);
        return null;
    }

    private Error HandleFailedPassword(User user)
    {
        _ = authRepository.IncrementFailedAttempts(user.Id);
        int failedAttemptsAfterCurrentFailure = user.FailedLoginAttempts + FailedLoginAttemptIncrement;
        logger.LogWarning("Failed login attempt for user {UserId}. Attempt {Attempt}/{Max}.", user.Id, failedAttemptsAfterCurrentFailure, MaxFailedAttempts);

        if (failedAttemptsAfterCurrentFailure < MaxFailedAttempts)
        {
            return Error.Unauthorized(code: "invalid_credentials", description: "Invalid email or password.");
        }

        if (authRepository.LockAccount(user.Id, DateTime.UtcNow.AddMinutes(LockoutMinutes)).IsError)
        {
            logger.LogError("Failed to lock account {UserId} after {Max} failed attempts.", user.Id, MaxFailedAttempts);
            return Error.Unauthorized(code: "invalid_credentials", description: "Too many failed attempts. Please try again later.");
        }

        logger.LogWarning("Account {UserId} locked for {Minutes} minutes after {Max} failed attempts.", user.Id, LockoutMinutes, MaxFailedAttempts);
        emailService.SendLockNotification(user.Email);
        return Error.Forbidden(code: "account_locked", description: "Account locked due to too many failed attempts.");
    }

    private ErrorOr<LoginSuccess> Handle2FA(User user)
    {
        ErrorOr<string> otpResult = GenerateOtpForMethod(user);
        if (otpResult.IsError)
        {
            logger.LogError("OTP generation failed for user {UserId}: {Error}", user.Id, otpResult.FirstError.Description);
            return otpResult.FirstError;
        }

        if (string.Equals(user.Preferred2FAMethod, EmailTwoFactorMethod, StringComparison.OrdinalIgnoreCase))
        {
            emailService.SendOTPCode(user.Email, otpResult.Value);
        }

        FailedOtpAttempts.Remove(user.Id);
        logger.LogInformation("2FA required for user {UserId} via {Method}.", user.Id, user.Preferred2FAMethod);
        return new RequiresTwoFactor(user.Id);
    }

    private ErrorOr<string> GenerateOtpForMethod(User user)
    {
        return string.Equals(user.Preferred2FAMethod, nameof(TwoFactorMethod.Authenticator), StringComparison.OrdinalIgnoreCase)
            ? otpService.GenerateTOTP(user.Id)
            : otpService.GenerateSMSOTP(user.Id);
    }

    private ErrorOr<bool> VerifyOtpForPreferredMethod(User user, string code)
    {
        return string.Equals(user.Preferred2FAMethod, nameof(TwoFactorMethod.Authenticator), StringComparison.OrdinalIgnoreCase)
            ? otpService.VerifyTOTP(user.Id, code)
            : otpService.VerifySMSOTP(user.Id, code);
    }

    private static int TrackFailedOtpAttempt(int userId)
    {
        FailedOtpAttempts.TryGetValue(userId, out int attempts);
        attempts++;
        FailedOtpAttempts[userId] = attempts;
        return attempts;
    }

    private ErrorOr<LoginSuccess> CompleteLogin(User user, SessionMetadata? metadata)
    {
        _ = authRepository.ResetFailedAttempts(user.Id);

        ErrorOr<string> tokenResult = jwtService.GenerateToken(user.Id);
        if (tokenResult.IsError)
        {
            logger.LogError("Token generation failed for user {UserId}: {Error}", user.Id, tokenResult.FirstError.Description);
            return tokenResult.FirstError;
        }

        string token = tokenResult.Value;

        if (authRepository.CreateSession(user.Id, token, metadata?.DeviceInfo, metadata?.Browser, metadata?.IpAddress).IsError)
        {
            logger.LogError("Session creation failed for user {UserId}.", user.Id);
            return Error.Failure(code: "session_failed", description: "Failed to create session.");
        }

        logger.LogInformation("User {UserId} logged in successfully.", user.Id);
        emailService.SendLoginAlert(user.Email);
        return new FullLogin(user.Id, token);
    }
}
