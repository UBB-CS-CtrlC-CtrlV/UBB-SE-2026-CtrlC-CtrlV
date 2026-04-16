namespace BankApp.Application.DTOs.Auth;

/// <summary>
/// Represents a request to initiate the forgot-password flow.
/// </summary>
public class ForgotPasswordRequest
{
    /// <summary>
    /// Gets or sets the email address to send the reset link to.
    /// </summary>
    public string Email { get; set; } = string.Empty;
}