namespace BankApp.Domain.Entities;

/// <summary>
/// Represents a password reset token issued to a user.
/// </summary>
public class PasswordResetToken
{
    /// <summary>
    /// Gets or sets the unique identifier for the token.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the user this token belongs to.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Gets or sets the hashed value of the reset token.
    /// </summary>
    public string TokenHash { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the date and time when the token expires.
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the token was used, if applicable.
    /// </summary>
    public DateTime? UsedAt { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the token was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }
}