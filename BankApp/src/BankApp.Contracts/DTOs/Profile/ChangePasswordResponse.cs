namespace BankApp.Contracts.DTOs.Profile;

/// <summary>
/// Represents the response returned after a password change attempt.
/// </summary>
public class ChangePasswordResponse
{
    /// <summary>
    /// Gets or sets a value indicating whether the password change was successful.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Gets or sets the message describing the result.
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ChangePasswordResponse"/> class.
    /// </summary>
    public ChangePasswordResponse()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ChangePasswordResponse"/> class.
    /// </summary>
    /// <param name="success">Whether the password change was successful.</param>
    /// <param name="message">A message describing the result.</param>
    public ChangePasswordResponse(bool success, string? message)
    {
        Success = success;
        Message = message;
    }
}