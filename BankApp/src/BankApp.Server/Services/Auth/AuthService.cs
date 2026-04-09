using BankApp.Contracts.DTOs.Auth;
using BankApp.Contracts.Entities;
using BankApp.Contracts.Enums;
using BankApp.Server.Repositories.Interfaces;
using BankApp.Server.Services.Notifications;
using BankApp.Server.Services.Security;
using BankApp.Server.Utilities;
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
    private static readonly string EmailTwoFactorMethod = TwoFactorMethod.Email.ToString();

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthService"/> class.
    /// </summary>
    /// <param name="authRepository">The authentication repository.</param>
    /// <param name="hashService">The password hashing service.</param>
    /// <param name="jwtService">The JWT token service.</param>
    /// <param name="otpService">The one-time password service.</param>
    /// <param name="emailService">The email delivery service.</param>
    public AuthService(IAuthRepository authRepository, IHashService hashService, IJwtService jwtService, IOtpService otpService, IEmailService emailService)
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

        var userResult = authRepository.FindUserByEmail(request.Email);
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

        if (user.Is2FAEnabled)
        {
            return Handle2FA(user);
        }

        return CompleteLogin(user);
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
        if (authRepository.CreateUser(user).IsError)
        {
            return new RegisterResponse { Success = false, Error = "Failed to create account." };
        }

        return new RegisterResponse { Success = true };
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

        var linkResult = authRepository.FindOAuthLink(request.Provider, providerUserId);
        User? user = null;

        if (!linkResult.IsError)
        {
            var userResult = authRepository.FindUserById(linkResult.Value.UserId);
            if (!userResult.IsError)
            {
                user = userResult.Value;
            }
        }

        if (user == null)
        {
            var byEmailResult = authRepository.FindUserByEmail(email);
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

                var createdResult = authRepository.FindUserByEmail(email);
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
            authRepository.CreateOAuthLink(newLink);
        }

        LoginResponse? lockCheck = CheckAccountLock(user);
        if (lockCheck != null)
        {
            return lockCheck;
        }

        if (user.Is2FAEnabled)
        {
            return Handle2FA(user);
        }

        return CompleteLogin(user);
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
            return new RegisterResponse { Success = false, Error = "This OAuth account is already registered. Please login." };
        }

        int targetUserId;
        var existingUserResult = authRepository.FindUserByEmail(request.Email);
        if (!existingUserResult.IsError)
        {
            targetUserId = existingUserResult.Value.Id;
        }
        else
        {
            string generatedTemporaryPassword = Guid.NewGuid().ToString() + TemporaryPasswordSuffix;
            User newUser = new User
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

            var savedUserResult = authRepository.FindUserByEmail(request.Email);
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

        if (authRepository.CreateOAuthLink(newLink).IsError)
        {
            return new RegisterResponse { Success = false, Error = "Failed to link OAuth account to user." };
        }

        return new RegisterResponse { Success = true };
    }

    /// <inheritdoc />
    public LoginResponse VerifyOTP(VerifyOTPRequest request)
    {
        var userResult = authRepository.FindUserById(request.UserId);
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
    public void ResendOTP(int userId, string method)
    {
        var userResult = authRepository.FindUserById(userId);
        if (userResult.IsError)
        {
            return;
        }

        User user = userResult.Value;
        string oneTimePassword = otpService.GenerateTOTP(user.Id);

        if (string.Equals(method, EmailTwoFactorMethod, StringComparison.OrdinalIgnoreCase)
            || string.Equals(user.Preferred2FAMethod, EmailTwoFactorMethod, StringComparison.OrdinalIgnoreCase))
        {
            emailService.SendOTPCode(user.Email, oneTimePassword);
        }
    }

    /// <inheritdoc />
    public void RequestPasswordReset(string email)
    {
        var userResult = authRepository.FindUserByEmail(email);
        if (userResult.IsError)
        {
            return;
        }

        User user = userResult.Value;
        authRepository.DeleteExpiredPasswordResetTokens();

        byte[] randomBytes = System.Security.Cryptography.RandomNumberGenerator.GetBytes(32);
        string rawToken = Convert.ToBase64String(randomBytes);
        string tokenHashForDb = ComputeSha256Hash(rawToken);

        PasswordResetToken resetToken = new PasswordResetToken
        {
            UserId = user.Id,
            TokenHash = tokenHashForDb,
            ExpiresAt = DateTime.UtcNow.AddMinutes(PasswordResetTokenExpiryMinutes),
            CreatedAt = DateTime.UtcNow,
        };

        authRepository.SavePasswordResetToken(resetToken);
        emailService.SendPasswordResetLink(user.Email, rawToken);
    }

    /// <inheritdoc />
    public ResetPasswordResult ResetPassword(string token, string newPassword)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return ResetPasswordResult.InvalidToken;
        }

        string tokenHash = ComputeSha256Hash(token);
        var tokenResult = authRepository.FindPasswordResetToken(tokenHash);
        if (tokenResult.IsError)
        {
            return ResetPasswordResult.InvalidToken;
        }

        PasswordResetToken resetToken = tokenResult.Value;
        ResetTokenValidationResult validationResult = this.GetResetTokenValidationResult(resetToken);
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

        authRepository.MarkPasswordResetTokenAsUsed(resetToken.Id);
        authRepository.InvalidateAllSessions(resetToken.UserId);

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
        var tokenResult = authRepository.FindPasswordResetToken(tokenHash);
        if (tokenResult.IsError)
        {
            return ResetTokenValidationResult.Invalid;
        }

        return this.GetResetTokenValidationResult(tokenResult.Value);
    }

    /// <inheritdoc />
    public bool Logout(string token)
    {
        var sessionResult = authRepository.FindSessionByToken(token);
        if (sessionResult.IsError)
        {
            return false;
        }

        authRepository.UpdateSessionToken(sessionResult.Value.Id);
        return true;
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

        authRepository.ResetFailedAttempts(user.Id);
        return null;
    }

    private LoginResponse HandleFailedPassword(User user)
    {
        authRepository.IncrementFailedAttempts(user.Id);

        if (user.FailedLoginAttempts + 1 >= MaxFailedAttempts)
        {
            authRepository.LockAccount(user.Id, DateTime.UtcNow.AddMinutes(LockoutMinutes));
            emailService.SendLockNotification(user.Email);
            return new LoginResponse { Success = false, Error = "Account locked due to too many failed attempts." };
        }

        return new LoginResponse { Success = false, Error = "Invalid email or password." };
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
        authRepository.ResetFailedAttempts(user.Id);
        string token = jwtService.GenerateToken(user.Id);
        authRepository.CreateSession(user.Id, token, null, null, null);
        emailService.SendLoginAlert(user.Email);
        return new LoginResponse
        {
            Success = true,
            Token = token,
            Requires2FA = false,
            UserId = user.Id,
        };
    }

    private string? ValidateRegistration(RegisterRequest request)
    {
        if (!ValidationUtilities.IsValidEmail(request.Email))
        {
            return "Invalid email format.";
        }

        if (!ValidationUtilities.IsStrongPassword(request.Password))
        {
            return "Password must be at least 8 characters with uppercase, lowercase, a digit, and a special character.";
        }

        if (string.IsNullOrWhiteSpace(request.FullName))
        {
            return "Full name is required.";
        }

        return null;
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

    private ResetTokenValidationResult GetResetTokenValidationResult(PasswordResetToken resetToken)
    {
        if (resetToken.UsedAt != null)
        {
            return ResetTokenValidationResult.AlreadyUsed;
        }

        if (resetToken.ExpiresAt < DateTime.UtcNow)
        {
            return ResetTokenValidationResult.Expired;
        }

        return ResetTokenValidationResult.Valid;
    }

    private string ComputeSha256Hash(string rawData)
    {
        byte[] bytes = System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(rawData));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
