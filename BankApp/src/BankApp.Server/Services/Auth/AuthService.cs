using BankApp.Contracts.DTOs.Auth;
using BankApp.Contracts.Entities;
using BankApp.Contracts.Enums;
using BankApp.Server.Repositories.Interfaces;
using BankApp.Server.Services.Notifications;
using BankApp.Server.Services.Security;
using BankApp.Server.Utilities;
using ErrorOr;
using Google.Apis.Auth;

namespace BankApp.Server.Services.Auth;

/// <summary>
/// Provides authentication, registration, OTP verification, and password management operations.
/// </summary>
public class AuthService : IAuthService
{
    private readonly IAuthRepository authRepository;
    private readonly IHashService hashService;
    private readonly IJwtService jwtService;
    private readonly IOtpService otpService;
    private readonly IEmailService emailService;

    private const int MaxFailedAttempts = 5;
    private const int LockoutMinutes = 30;
    private const int PasswordResetTokenExpiryMinutes = 5;
    private const string GoogleOAuthProvider = "Google";
    private const string DefaultLanguage = "en";
    private const string TemporaryPasswordSuffix = "A1a!";
    private const string EmailTwoFactorMethod = nameof(TwoFactorMethod.Email);

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthService"/> class.
    /// </summary>
    /// <param name="authRepository">The authentication repository.</param>
    /// <param name="hashService">The password hashing service.</param>
    /// <param name="jwtService">The JWT token service.</param>
    /// <param name="otpService">The one-time password service.</param>
    /// <param name="emailService">The email delivery service.</param>
    public AuthService(IAuthRepository authRepository, IHashService hashService, IJwtService jwtService,
        IOtpService otpService, IEmailService emailService)
    {
        this.authRepository = authRepository;
        this.hashService = hashService;
        this.jwtService = jwtService;
        this.otpService = otpService;
        this.emailService = emailService;
    }

    /// <inheritdoc />
    public LoginResponse Login(LoginRequest request)
    {
        if (!ValidationUtilities.IsValidEmail(request.Email))
        {
            return new LoginResponse { Success = false, Error = "Invalid mail format." };
        }

        ErrorOr<User> userResult = authRepository.FindUserByEmail(request.Email);
        if (userResult.IsError)
        {
            return new LoginResponse { Success = false, Error = "Invalid email or password." };
        }

        User user = userResult.Value;

        LoginResponse? lockCheck = CheckAccountLock(user);
        if (lockCheck != null)
        {
            return lockCheck;
        }

        if (!hashService.Verify(request.Password, user.PasswordHash))
        {
            return HandleFailedPassword(user);
        }

        return user.Is2FAEnabled ? Handle2FA(user) : CompleteLogin(user);
    }

    /// <inheritdoc />
    public RegisterResponse Register(RegisterRequest request)
    {
        string? validationError = ValidateRegistration(request);
        if (validationError != null)
        {
            return new RegisterResponse { Success = false, Error = validationError };
        }

        if (!authRepository.FindUserByEmail(request.Email).IsError)
        {
            return new RegisterResponse { Success = false, Error = "Email is already registered." };
        }

        User user = CreateUserFromRequest(request);
        return authRepository.CreateUser(user).IsError
            ? new RegisterResponse { Success = false, Error = "Failed to create account." }
            : new RegisterResponse { Success = true };
    }

    /// <inheritdoc />
    public async Task<LoginResponse> OAuthLoginAsync(OAuthLoginRequest request)
    {
        if (!request.Provider.Equals(GoogleOAuthProvider, StringComparison.OrdinalIgnoreCase))
        {
            return new LoginResponse { Success = false, Error = "Unsupported OAuth Provider." };
        }

        GoogleJsonWebSignature.Payload payload;
        try
        {
            payload = await GoogleJsonWebSignature.ValidateAsync(request.ProviderToken);
        }
        catch (InvalidJwtException)
        {
            return new LoginResponse { Success = false, Error = "Invalid Google authentication token." };
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
                var newUser = new User
                {
                    Email = email,
                    PasswordHash = hashService.GetHash(generatedTemporaryPassword),
                    FullName = fullName,
                    PreferredLanguage = DefaultLanguage,
                    Is2FAEnabled = false,
                    IsLocked = false,
                    FailedLoginAttempts = 0,
                };

                if (authRepository.CreateUser(newUser).IsError)
                {
                    return new LoginResponse { Success = false, Error = "Failed to create user account." };
                }

                ErrorOr<User> createdResult = authRepository.FindUserByEmail(email);
                if (createdResult.IsError)
                {
                    return new LoginResponse { Success = false, Error = "Failed to retrieve created user." };
                }

                user = createdResult.Value;
            }

            var newLink = new OAuthLink
            {
                UserId = user.Id,
                Provider = request.Provider,
                ProviderUserId = providerUserId,
                ProviderEmail = email,
            };
            if (authRepository.CreateOAuthLink(newLink).IsError)
            {
                return new LoginResponse { Success = false, Error = "Failed to link OAuth account." };
            }
        }

        LoginResponse? lockCheck = CheckAccountLock(user);
        if (lockCheck != null)
        {
            return lockCheck;
        }

        return user.Is2FAEnabled ? Handle2FA(user) : CompleteLogin(user);
    }

    /// <inheritdoc />
    public RegisterResponse OAuthRegister(OAuthRegisterRequest request)
    {
        if (!ValidationUtilities.IsValidEmail(request.Email))
        {
            return new RegisterResponse { Success = false, Error = "Invalid email format." };
        }

        if (!authRepository.FindOAuthLink(request.Provider, request.ProviderToken).IsError)
        {
            return new RegisterResponse
                { Success = false, Error = "This OAuth account is already registered. Please login." };
        }

        int targetUserId;
        ErrorOr<User> existingUserResult = authRepository.FindUserByEmail(request.Email);
        if (!existingUserResult.IsError)
        {
            targetUserId = existingUserResult.Value.Id;
        }
        else
        {
            string generatedTemporaryPassword = Guid.NewGuid().ToString() + TemporaryPasswordSuffix;
            var newUser = new User
            {
                Email = request.Email,
                PasswordHash = hashService.GetHash(generatedTemporaryPassword),
                FullName = request.FullName,
                PreferredLanguage = DefaultLanguage,
                Is2FAEnabled = false,
                IsLocked = false,
                FailedLoginAttempts = 0,
            };

            if (authRepository.CreateUser(newUser).IsError)
            {
                return new RegisterResponse { Success = false, Error = "Failed to create user account." };
            }

            ErrorOr<User> savedUserResult = authRepository.FindUserByEmail(request.Email);
            if (savedUserResult.IsError)
            {
                return new RegisterResponse { Success = false, Error = "Error retrieving created user." };
            }

            targetUserId = savedUserResult.Value.Id;
        }

        var newLink = new OAuthLink
        {
            UserId = targetUserId,
            Provider = request.Provider,
            ProviderUserId = request.ProviderToken,
            ProviderEmail = request.Email,
        };

        return authRepository.CreateOAuthLink(newLink).IsError
            ? new RegisterResponse { Success = false, Error = "Failed to link OAuth account to user." }
            : new RegisterResponse { Success = true };
    }

    /// <inheritdoc />
    public LoginResponse VerifyOTP(VerifyOTPRequest request)
    {
        ErrorOr<User> userResult = authRepository.FindUserById(request.UserId);
        if (userResult.IsError)
        {
            return new LoginResponse { Success = false, Error = "User not found." };
        }

        User user = userResult.Value;

        if (!otpService.VerifyTOTP(request.UserId, request.OTPCode))
        {
            return new LoginResponse { Success = false, Error = "Invalid or expired OTP code." };
        }

        otpService.InvalidateOTP(user.Id);
        return CompleteLogin(user);
    }

    /// <inheritdoc />
    public ErrorOr<Success> ResendOTP(int userId, string method)
    {
        ErrorOr<User> userResult = authRepository.FindUserById(userId);
        if (userResult.IsError)
        {
            return userResult.FirstError;
        }

        User user = userResult.Value;
        string oneTimePassword = otpService.GenerateTOTP(user.Id);

        if (string.Equals(method, EmailTwoFactorMethod, StringComparison.OrdinalIgnoreCase)
            || string.Equals(user.Preferred2FAMethod, EmailTwoFactorMethod, StringComparison.OrdinalIgnoreCase))
        {
            emailService.SendOTPCode(user.Email, oneTimePassword);
        }

        return Result.Success;
    }

    /// <inheritdoc />
    public ErrorOr<Success> RequestPasswordReset(string email)
    {
        ErrorOr<User> userResult = authRepository.FindUserByEmail(email);
        if (userResult.IsError)
        {
            return userResult.FirstError;
        }

        User user = userResult.Value;
        _ = authRepository.DeleteExpiredPasswordResetTokens();

        byte[] randomBytes = System.Security.Cryptography.RandomNumberGenerator.GetBytes(32);
        string rawToken = Convert.ToBase64String(randomBytes);
        string tokenHashForDb = ComputeSha256Hash(rawToken);

        var resetToken = new PasswordResetToken
        {
            UserId = user.Id,
            TokenHash = tokenHashForDb,
            ExpiresAt = DateTime.UtcNow.AddMinutes(PasswordResetTokenExpiryMinutes),
            CreatedAt = DateTime.UtcNow,
        };

        if (authRepository.SavePasswordResetToken(resetToken).IsError)
        {
            return Error.Failure(description: "Failed to save password reset token.");
        }

        emailService.SendPasswordResetLink(user.Email, rawToken);
        return Result.Success;
    }

    /// <inheritdoc />
    public ResetPasswordResult ResetPassword(string token, string newPassword)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return ResetPasswordResult.InvalidToken;
        }

        string tokenHash = ComputeSha256Hash(token);
        ErrorOr<PasswordResetToken> tokenResult = authRepository.FindPasswordResetToken(tokenHash);
        if (tokenResult.IsError)
        {
            return ResetPasswordResult.InvalidToken;
        }

        PasswordResetToken resetToken = tokenResult.Value;
        ResetTokenValidationResult validationResult = GetResetTokenValidationResult(resetToken);
        if (validationResult != ResetTokenValidationResult.Valid)
        {
            return validationResult switch
            {
                ResetTokenValidationResult.Expired => ResetPasswordResult.ExpiredToken,
                ResetTokenValidationResult.AlreadyUsed => ResetPasswordResult.TokenAlreadyUsed,
                _ => ResetPasswordResult.InvalidToken,
            };
        }

        string finalPasswordHash = hashService.GetHash(newPassword);
        if (authRepository.UpdatePassword(resetToken.UserId, finalPasswordHash).IsError)
        {
            return ResetPasswordResult.InvalidToken;
        }

        if (authRepository.MarkPasswordResetTokenAsUsed(resetToken.Id).IsError)
        {
            return ResetPasswordResult.Failed;
        }

        if (authRepository.InvalidateAllSessions(resetToken.UserId).IsError)
        {
            return ResetPasswordResult.Failed;
        }

        return ResetPasswordResult.Success;
    }

    /// <inheritdoc />
    public ResetTokenValidationResult VerifyResetToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return ResetTokenValidationResult.Invalid;
        }

        string tokenHash = ComputeSha256Hash(token);
        ErrorOr<PasswordResetToken> tokenResult = authRepository.FindPasswordResetToken(tokenHash);
        return tokenResult.IsError
            ? ResetTokenValidationResult.Invalid
            : GetResetTokenValidationResult(tokenResult.Value);
    }

    /// <inheritdoc />
    public ErrorOr<Success> Logout(string token)
    {
        ErrorOr<Session> sessionResult = authRepository.FindSessionByToken(token);
        if (sessionResult.IsError)
        {
            return sessionResult.FirstError;
        }

        authRepository.UpdateSessionToken(sessionResult.Value.Id);
        return Result.Success;
    }

    private LoginResponse? CheckAccountLock(User user)
    {
        if (!user.IsLocked)
        {
            return null;
        }

        if (user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTime.UtcNow)
        {
            return new LoginResponse { Success = false, Error = "Account is locked. Try again later." };
        }

        _ = authRepository.ResetFailedAttempts(user.Id);
        return null;
    }

    private LoginResponse HandleFailedPassword(User user)
    {
        _ = authRepository.IncrementFailedAttempts(user.Id);

        if (user.FailedLoginAttempts + 1 < MaxFailedAttempts)
        {
            return new LoginResponse { Success = false, Error = "Invalid email or password." };
        }

        if (authRepository.LockAccount(user.Id, DateTime.UtcNow.AddMinutes(LockoutMinutes)).IsError)
        {
            return new LoginResponse { Success = false, Error = "Too many failed attempts. Please try again later." };
        }

        emailService.SendLockNotification(user.Email);
        return new LoginResponse { Success = false, Error = "Account locked due to too many failed attempts." };
    }

    private LoginResponse Handle2FA(User user)
    {
        string oneTimePassword = otpService.GenerateTOTP(user.Id);

        if (string.Equals(user.Preferred2FAMethod, EmailTwoFactorMethod, StringComparison.OrdinalIgnoreCase))
        {
            emailService.SendOTPCode(user.Email, oneTimePassword);
        }

        return new LoginResponse
        {
            Success = true,
            Requires2FA = true,
            UserId = user.Id,
            Token = null,
        };
    }

    private LoginResponse CompleteLogin(User user)
    {
        _ = authRepository.ResetFailedAttempts(user.Id);
        string token = jwtService.GenerateToken(user.Id);

        if (authRepository.CreateSession(user.Id, token, null, null, null).IsError)
        {
            return new LoginResponse { Success = false, Error = "Failed to create session." };
        }

        emailService.SendLoginAlert(user.Email);
        return new LoginResponse
        {
            Success = true,
            Token = token,
            Requires2FA = false,
            UserId = user.Id,
        };
    }

    private static string? ValidateRegistration(RegisterRequest request)
    {
        if (!ValidationUtilities.IsValidEmail(request.Email))
        {
            return "Invalid email format.";
        }

        if (!ValidationUtilities.IsStrongPassword(request.Password))
        {
            return
                "Password must be at least 8 characters with uppercase, lowercase, a digit, and a special character.";
        }

        return string.IsNullOrWhiteSpace(request.FullName) ? "Full name is required." : null;
    }

    private User CreateUserFromRequest(RegisterRequest request)
    {
        return new User
        {
            Email = request.Email,
            PasswordHash = hashService.GetHash(request.Password),
            FullName = request.FullName,
            PreferredLanguage = DefaultLanguage,
            Is2FAEnabled = false,
            IsLocked = false,
            FailedLoginAttempts = 0,
        };
    }

    private static ResetTokenValidationResult GetResetTokenValidationResult(PasswordResetToken resetToken)
    {
        if (resetToken.UsedAt != null)
        {
            return ResetTokenValidationResult.AlreadyUsed;
        }

        return resetToken.ExpiresAt < DateTime.UtcNow
            ? ResetTokenValidationResult.Expired
            : ResetTokenValidationResult.Valid;
    }

    private static string ComputeSha256Hash(string rawData)
    {
        byte[] bytes = System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(rawData));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}