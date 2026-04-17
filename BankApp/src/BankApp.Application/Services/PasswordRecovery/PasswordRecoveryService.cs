// <copyright file="PasswordRecoveryService.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

using BankApp.Domain.Entities;
using BankApp.Application.Repositories.Interfaces;
using BankApp.Application.Services.Notifications;
using BankApp.Application.Services.Security;
using ErrorOr;
using Microsoft.Extensions.Logging;

namespace BankApp.Application.Services.PasswordRecovery;

/// <summary>
/// Provides password reset and recovery operations.
/// </summary>
public class PasswordRecoveryService : IPasswordRecoveryService
{
    private readonly IAuthRepository authRepository;
    private readonly IHashService hashService;
    private readonly IEmailService emailService;
    private readonly ILogger<PasswordRecoveryService> logger;

    private const int PasswordResetTokenExpiryMinutes = 30;

    /// <summary>
    /// Initializes a new instance of the <see cref="PasswordRecoveryService"/> class.
    /// </summary>
    /// <param name="authRepository">The authentication repository.</param>
    /// <param name="hashService">The password hashing service.</param>
    /// <param name="emailService">The email delivery service.</param>
    /// <param name="logger">The logger.</param>
    public PasswordRecoveryService(IAuthRepository authRepository, IHashService hashService,
        IEmailService emailService, ILogger<PasswordRecoveryService> logger)
    {
        this.authRepository = authRepository;
        this.hashService = hashService;
        this.emailService = emailService;
        this.logger = logger;
    }

    /// <inheritdoc />
    public ErrorOr<Success> RequestPasswordReset(string email)
    {
        ErrorOr<User> userResult = authRepository.FindUserByEmail(email);
        if (userResult.IsError)
        {
            logger.LogInformation("Password reset requested: no account found.");
            return userResult.FirstError;
        }

        User user = userResult.Value;
        _ = authRepository.DeleteExpiredPasswordResetTokens();

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

        if (authRepository.SavePasswordResetToken(resetToken).IsError)
        {
            logger.LogError("Failed to save password reset token for user {UserId}.", user.Id);
            return Error.Failure(description: "Failed to save password reset token.");
        }

        logger.LogInformation("Password reset email sent for user {UserId}.", user.Id);
        emailService.SendPasswordResetLink(user.Email, rawToken);
        return Result.Success;
    }

    /// <inheritdoc />
    public ErrorOr<Success> ResetPassword(string token, string newPassword)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return Error.Validation(code: "token_invalid", description: "The reset token is invalid.");
        }

        string tokenHash = ComputeSha256Hash(token);
        ErrorOr<PasswordResetToken> tokenResult = authRepository.FindPasswordResetToken(tokenHash);
        if (tokenResult.IsError)
        {
            logger.LogWarning("Password reset failed: token not found.");
            return Error.Validation(code: "token_invalid", description: "The reset token is invalid.");
        }

        PasswordResetToken resetToken = tokenResult.Value;
        ErrorOr<Success> validationResult = ValidateResetToken(resetToken);
        if (validationResult.IsError)
        {
            logger.LogWarning("Password reset failed for user {UserId}: {Code}.", resetToken.UserId, validationResult.FirstError.Code);
            return validationResult.FirstError;
        }

        ErrorOr<string> hashResult = hashService.GetHash(newPassword);
        if (hashResult.IsError)
        {
            logger.LogError("Hash generation failed during password reset for user {UserId}.", resetToken.UserId);
            return hashResult.FirstError;
        }

        if (authRepository.UpdatePassword(resetToken.UserId, hashResult.Value).IsError)
        {
            logger.LogError("Password update failed for user {UserId}.", resetToken.UserId);
            return Error.Validation(code: "token_invalid", description: "Failed to update password.");
        }

        if (authRepository.MarkPasswordResetTokenAsUsed(resetToken.Id).IsError)
        {
            logger.LogError("Failed to mark password reset token as used for user {UserId}. Token may be replayable.", resetToken.UserId);
            return Error.Failure(code: "reset_failed", description: "Password was updated but the token could not be invalidated.");
        }

        if (authRepository.InvalidateAllSessions(resetToken.UserId).IsError)
        {
            logger.LogError("Failed to invalidate sessions for user {UserId} after password reset. Active sessions may remain valid.", resetToken.UserId);
            return Error.Failure(code: "reset_failed", description: "Password was updated but active sessions could not be invalidated.");
        }

        logger.LogInformation("Password reset successfully for user {UserId}.", resetToken.UserId);
        return Result.Success;
    }

    /// <inheritdoc />
    public ErrorOr<Success> VerifyResetToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return Error.Validation(code: "token_invalid", description: "The reset token is invalid.");
        }

        string tokenHash = ComputeSha256Hash(token);
        ErrorOr<PasswordResetToken> tokenResult = authRepository.FindPasswordResetToken(tokenHash);
        if (tokenResult.IsError)
        {
            return Error.Validation(code: "token_invalid", description: "The reset token is invalid.");
        }

        return ValidateResetToken(tokenResult.Value);
    }

    private static ErrorOr<Success> ValidateResetToken(PasswordResetToken resetToken)
    {
        if (resetToken.UsedAt != null)
        {
            return Error.Validation(code: "token_already_used", description: "The reset token has already been used.");
        }

        if (resetToken.ExpiresAt < DateTime.UtcNow)
        {
            return Error.Validation(code: "token_expired", description: "The reset token has expired.");
        }

        return Result.Success;
    }

    private static string ComputeSha256Hash(string rawData)
    {
        byte[] bytes = System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(rawData));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
