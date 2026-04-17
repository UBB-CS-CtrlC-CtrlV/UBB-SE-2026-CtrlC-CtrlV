namespace BankApp.Application.DataTransferObjects.Auth;

/// <summary>
/// Represents a login request using an OAuth provider token.
/// </summary>
public class OAuthLoginRequest
{
    /// <summary>
    /// Gets or sets the OAuth provider name (e.g. Google, Facebook).
    /// </summary>
    public string Provider { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the token received from the OAuth provider.
    /// </summary>
    public string ProviderToken { get; set; } = string.Empty;
}