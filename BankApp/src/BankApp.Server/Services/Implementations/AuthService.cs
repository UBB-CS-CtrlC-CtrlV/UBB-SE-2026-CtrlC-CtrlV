using BankApp.Core.DTOs.Auth;
using BankApp.Core.Entities;
using BankApp.Core.Enums;
using BankApp.Infrastructure.Repositories.Interfaces;
using BankApp.Infrastructure.Services.Infrastructure.Implementations;
using BankApp.Infrastructure.Services.Infrastructure.Interfaces;
using BankApp.Infrastructure.Services.Interfaces;
using BankApp.Infrastructure.Utilities;
using BankApp.Server.Services.Infrastructure.Interfaces;
using Google.Apis.Auth;

namespace BankApp.Infrastructure.Services.Implementations
{
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
        private const int OtpRangeMinimum = 100000;
        private const int OtpRangeMaximum = 999999;

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

            User? user = authRepository.FindUserByEmail(request.Email);
            if (user == null)
            {
                return new LoginResponse { Success = false, Error = "Invalid email or password." };
            }

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

            User? existingUser = authRepository.FindUserByEmail(request.Email);
            if (existingUser != null)
            {
                return new RegisterResponse { Success = false, Error = "Email is already registered." };
            }

            User user = CreateUserFromRequest(request);
            bool created = authRepository.CreateUser(user);

            if (!created)
            {
                return new RegisterResponse { Success = false, Error = "Failed to create account." };
            }

            return new RegisterResponse { Success = true };
        }

        /// <inheritdoc />
        public async Task<LoginResponse> OAuthLoginAsync(OAuthLoginRequest request)
        {
            if (request.Provider.Equals("Google", StringComparison.OrdinalIgnoreCase))
            {
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

                OAuthLink? link = authRepository.FindOAuthLink(request.Provider, providerUserId);
                User? user = null;

                if (link != null)
                {
                    user = authRepository.FindUserById(link.UserId);
                }

                if (user == null)
                {
                    user = authRepository.FindUserByEmail(email);
                    if (user == null)
                    {
                        string randomPassword = Guid.NewGuid().ToString() + "A1a!";
                        user = new User
                        {
                            Email = email,
                            PasswordHash = hashService.GetHash(randomPassword),
                            FullName = fullName,
                            PreferredLanguage = "en",
                            Is2FAEnabled = false,
                            IsLocked = false,
                            FailedLoginAttempts = 0
                        };

                        if (!authRepository.CreateUser(user))
                        {
                            return new LoginResponse { Success = false, Error = "Failed to create user account." };
                        }

                        user = authRepository.FindUserByEmail(email);
                    }

                    OAuthLink newLink = new OAuthLink
                    {
                        UserId = user!.Id,
                        Provider = request.Provider,
                        ProviderUserId = providerUserId,
                        ProviderEmail = email
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

            return new LoginResponse { Success = false, Error = "Unsupported OAuth Provider." };
        }

        /// <inheritdoc />
        public RegisterResponse OAuthRegister(OAuthRegisterRequest request)
        {
            if (!ValidationUtilities.IsValidEmail(request.Email))
            {
                return new RegisterResponse { Success = false, Error = "Invalid email format." };
            }

            OAuthLink? existingLink = authRepository.FindOAuthLink(request.Provider, request.ProviderToken);
            if (existingLink != null)
            {
                return new RegisterResponse { Success = false, Error = "This OAuth account is already registered. Please login." };
            }

            User? existingUser = authRepository.FindUserByEmail(request.Email);
            int targetUserId;
            if (existingUser != null)
            {
                targetUserId = existingUser.Id;
            }
            else
            {
                string randomPassword = Guid.NewGuid().ToString() + "A1a!";
                User newUser = new User
                {
                    Email = request.Email,
                    PasswordHash = hashService.GetHash(randomPassword),
                    FullName = request.FullName,
                    PreferredLanguage = "en",
                    Is2FAEnabled = false,
                    IsLocked = false,
                    FailedLoginAttempts = 0
                };

                bool created = authRepository.CreateUser(newUser);
                if (!created)
                {
                    return new RegisterResponse { Success = false, Error = "Failed to create user account." };
                }

                User? savedUser = authRepository.FindUserByEmail(request.Email);
                if (savedUser == null)
                {
                    return new RegisterResponse { Success = false, Error = "Error retrieving created user." };
                }

                targetUserId = savedUser.Id;
            }

            OAuthLink newLink = new OAuthLink
            {
                UserId = targetUserId,
                Provider = request.Provider,
                ProviderUserId = request.ProviderToken,
                ProviderEmail = request.Email
            };

            bool linkCreated = authRepository.CreateOAuthLink(newLink);
            if (!linkCreated)
            {
                return new RegisterResponse { Success = false, Error = "Failed to link OAuth account to user." };
            }

            return new RegisterResponse { Success = true };
        }

        /// <inheritdoc />
        public LoginResponse VerifyOTP(VerifyOTPRequest request)
        {
            User? user = authRepository.FindUserById(request.UserId);
            if (user == null)
            {
                return new LoginResponse { Success = false, Error = "User not found." };
            }
            bool isValid = otpService.VerifyTOTP(request.UserId, request.OTPCode);
            if (!isValid)
            {
                return new LoginResponse { Success = false, Error = "Invalid or expired OTP code." };
            }
            otpService.InvalidateOTP(user.Id);
            return CompleteLogin(user);
        }

        /// <inheritdoc />
        public void ResendOTP(int userId, string method)
        {
            User? user = authRepository.FindUserById(userId);
            if (user == null)
            {
                return;
            }

            string oneTimePassword = otpService.GenerateTOTP(user.Id);
            if (method == "email" || user.Preferred2FAMethod == "email")
            {
                emailService.SendOTPCode(user.Email, oneTimePassword);
            }
        }

        /// <inheritdoc />
        public void RequestPasswordReset(string email)
        {
            User? user = authRepository.FindUserByEmail(email);
            if (user == null)
            {
                return;
            }

            authRepository.DeleteExpiredPasswordResetTokens();

            byte[] randomBytes = System.Security.Cryptography.RandomNumberGenerator.GetBytes(32);
            string rawToken = Convert.ToBase64String(randomBytes);
            string tokenHashForDb = ComputeSha256Hash(rawToken);

            PasswordResetToken resetToken = new PasswordResetToken
            {
                UserId = user.Id,
                TokenHash = tokenHashForDb,
                ExpiresAt = DateTime.UtcNow.AddMinutes(PasswordResetTokenExpiryMinutes),
                CreatedAt = DateTime.UtcNow
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
            PasswordResetToken? resetToken = authRepository.FindPasswordResetToken(tokenHash);
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
            bool updated = authRepository.UpdatePassword(resetToken!.UserId, finalPasswordHash);

            if (!updated)
            {
                return ResetPasswordResult.InvalidToken;
            }

            authRepository.MarkPasswordResetTokenAsUsed(resetToken.Id);
            authRepository.InvalidateAllSessions(resetToken.UserId);

            return ResetPasswordResult.Success;
        }

        // PRIVATE HELPERS
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

            // Lockout expired, reset and allow login attempt
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

            if (user.Preferred2FAMethod == "email")
            {
                emailService.SendOTPCode(user.Email, oneTimePassword);
            }

            return new LoginResponse
            {
                Success = true,
                Requires2FA = true,
                UserId = user.Id,
                Token = null
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
                UserId = user.Id
            };
        }

        private string? ValidateRegistration(RegisterRequest request)
        {
            // There should also be client-side validation, this is last resort
            // can't trust the client
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
                PreferredLanguage = "en",
                Is2FAEnabled = false,
                IsLocked = false,
                FailedLoginAttempts = 0
            };
        }

        /// <inheritdoc />
        public ResetTokenValidationResult VerifyResetToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return ResetTokenValidationResult.Invalid;
            }

            string tokenHash = ComputeSha256Hash(token);
            PasswordResetToken? resetToken = authRepository.FindPasswordResetToken(tokenHash);
            return this.GetResetTokenValidationResult(resetToken);
        }

        private ResetTokenValidationResult GetResetTokenValidationResult(PasswordResetToken? resetToken)
        {
            if (resetToken == null)
            {
                return ResetTokenValidationResult.Invalid;
            }

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

        /// <inheritdoc />
        public bool Logout(string token)
        {
            Session? session = authRepository.FindSessionByToken(token);
            if (session == null)
            {
                return false;
            }
            authRepository.UpdateSessionToken(session.Id);
            return true;
        }

        private string ComputeSha256Hash(string rawData)
        {
            byte[] bytes = System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(rawData));
            return Convert.ToHexString(bytes).ToLowerInvariant();
        }
    }
}

