// <copyright file="IOTPService.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

using ErrorOr;

namespace BankApp.Application.Services.Security;

/// <summary>
/// Defines operations for generating and verifying one-time passwords.
/// </summary>
public interface IOtpService   // To Do: Change to OTP
{
    /// <summary>
    /// Generates a time-based one-time password for the specified user.
    /// </summary>
    /// <param name="userId">The identifier of the user.</param>
    /// <returns>
    /// The generated TOTP code on success,
    /// or a failure error if the underlying HMAC operation throws.
    /// </returns>
    ErrorOr<string> GenerateTOTP(int userId);

    /// <summary>
    /// Verifies a time-based one-time password for the specified user.
    /// </summary>
    /// <param name="userId">The identifier of the user.</param>
    /// <param name="code">The TOTP code to verify.</param>
    /// <returns>
    /// <see langword="true"/> if the code is valid for the current or previous window,
    /// <see langword="false"/> if it does not match,
    /// or a failure error if the underlying HMAC operation throws.
    /// </returns>
    ErrorOr<bool> VerifyTOTP(int userId, string code);

    /// <summary>
    /// Generates an SMS one-time password for the specified user.
    /// </summary>
    /// <param name="userId">The identifier of the user.</param>
    /// <returns>
    /// The generated SMS OTP code on success,
    /// or a failure error if the underlying random number generation throws.
    /// </returns>
    ErrorOr<string> GenerateSMSOTP(int userId);

    /// <summary>
    /// Verifies an SMS one-time password for the specified user.
    /// </summary>
    /// <param name="userId">The identifier of the user.</param>
    /// <param name="code">The SMS OTP code to verify.</param>
    /// <returns>
    /// <see langword="true"/> if the code is valid and not expired,
    /// <see langword="false"/> if it does not match or has expired,
    /// or a failure error if an unexpected exception occurs.
    /// </returns>
    ErrorOr<bool> VerifySMSOTP(int userId, string code);

    /// <summary>
    /// Determines whether a token has expired.
    /// </summary>
    /// <param name="expiredAt">The expiration time to check.</param>
    /// <returns><see langword="true"/> if the current time is past the expiration; otherwise, <see langword="false"/>.</returns>
    bool IsExpired(DateTime expiredAt);

    /// <summary>
    /// Invalidates any stored OTP for the specified user.
    /// </summary>
    /// <param name="userId">The identifier of the user.</param>
    void InvalidateOTP(int userId);
}
