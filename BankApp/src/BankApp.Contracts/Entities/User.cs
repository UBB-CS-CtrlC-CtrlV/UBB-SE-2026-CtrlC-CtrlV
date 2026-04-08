namespace BankApp.Contracts.Entities;

/// <summary>
/// Represents a user of the banking application.
/// </summary>
public class User
{
    /// <summary>
    /// Gets or sets the unique identifier for the user.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the email address of the user.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the hashed password of the user.
    /// </summary>
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the full name of the user.
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the phone number of the user.
    /// </summary>
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// Gets or sets the date of birth of the user.
    /// </summary>
    public DateTime? DateOfBirth { get; set; }

    /// <summary>
    /// Gets or sets the address of the user.
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// Gets or sets the nationality of the user.
    /// </summary>
    public string? Nationality { get; set; }

    /// <summary>
    /// Gets or sets the preferred language of the user.
    /// </summary>
    public string PreferredLanguage { get; set; } = "en";

    /// <summary>
    /// Gets or sets a value indicating whether two-factor authentication is enabled.
    /// </summary>
    public bool Is2FAEnabled { get; set; }

    /// <summary>
    /// Gets or sets the preferred two-factor authentication method.
    /// </summary>
    public string? Preferred2FAMethod { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the user account is locked.
    /// </summary>
    public bool IsLocked { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the lockout ends, if applicable.
    /// </summary>
    public DateTime? LockoutEnd { get; set; }

    /// <summary>
    /// Gets or sets the number of consecutive failed login attempts.
    /// </summary>
    public int FailedLoginAttempts { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the user was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the user was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}