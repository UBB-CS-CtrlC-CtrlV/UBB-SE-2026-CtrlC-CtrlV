namespace BankApp.Contracts.DTOs.Auth;

/// <summary>
/// Represents a request to verify a one-time password during two-factor authentication.
/// </summary>
public class VerifyOTPRequest
{
    /// <summary>
    /// Gets or sets the identifier of the user verifying the OTP.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Gets or sets the one-time password code to verify.
    /// </summary>
    public string OTPCode { get; set; } = string.Empty;
}