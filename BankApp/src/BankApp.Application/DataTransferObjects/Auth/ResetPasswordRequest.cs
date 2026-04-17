namespace BankApp.Application.DataTransferObjects.Auth;

/// <summary>
/// Represents a request to reset a password using a reset token.
/// </summary>
public class ResetPasswordRequest
{
    /// <summary>
    /// Gets or sets the reset token.
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the new password.
    /// </summary>
    public string NewPassword { get; set; } = string.Empty;
}