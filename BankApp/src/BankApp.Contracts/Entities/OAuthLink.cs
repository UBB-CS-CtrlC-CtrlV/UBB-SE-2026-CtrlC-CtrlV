namespace BankApp.Contracts.Entities;

/// <summary>
/// Represents an OAuth provider link for a user account.
/// </summary>
public class OAuthLink
{
    /// <summary>
    /// Gets or sets the unique identifier for the OAuth link.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the user who owns this link.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Gets or sets the OAuth provider name (e.g. Google, Facebook).
    /// </summary>
    public string Provider { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user identifier from the OAuth provider.
    /// </summary>
    public string ProviderUserId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the email address associated with the OAuth provider account.
    /// </summary>
    public string? ProviderEmail { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the OAuth link was created.
    /// </summary>
    public DateTime LinkedAt { get; set; }
}