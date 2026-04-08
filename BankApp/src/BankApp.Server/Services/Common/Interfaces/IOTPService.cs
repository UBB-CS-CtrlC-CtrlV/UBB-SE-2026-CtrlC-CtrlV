namespace BankApp.Server.Services.Common.Interfaces;

/// <summary>
/// Defines operations for generating and verifying one-time passwords.
/// </summary>
public interface IOtpService
{
    /// <summary>
    /// Generates a time-based one-time password for the specified user.
    /// </summary>
    /// <param name="userId">The identifier of the user.</param>
    /// <returns>The generated TOTP code.</returns>
    string GenerateTOTP(int userId);

    /// <summary>
    /// Verifies a time-based one-time password for the specified user.
    /// </summary>
    /// <param name="userId">The identifier of the user.</param>
    /// <param name="code">The TOTP code to verify.</param>
    /// <returns><see langword="true"/> if the code is valid; otherwise, <see langword="false"/>.</returns>
    bool VerifyTOTP(int userId, string code);

    /// <summary>
    /// Generates an SMS one-time password for the specified user.
    /// </summary>
    /// <param name="userId">The identifier of the user.</param>
    /// <returns>The generated SMS OTP code.</returns>
    string GenerateSMSOTP(int userId);

    /// <summary>
    /// Verifies an SMS one-time password for the specified user.
    /// </summary>
    /// <param name="userId">The identifier of the user.</param>
    /// <param name="code">The SMS OTP code to verify.</param>
    /// <returns><see langword="true"/> if the code is valid; otherwise, <see langword="false"/>.</returns>
    bool VerifySMSOTP(int userId, string code);

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
