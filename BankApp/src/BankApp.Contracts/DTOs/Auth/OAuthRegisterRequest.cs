namespace BankApp.Core.DTOs.Auth
{
    /// <summary>
    /// Represents a registration request using an OAuth provider token.
    /// </summary>
    public class OAuthRegisterRequest
    {
        /// <summary>
        /// Gets or sets the OAuth provider name (e.g. Google, Facebook).
        /// </summary>
        public string Provider { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the token received from the OAuth provider.
        /// </summary>
        public string ProviderToken { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the full name of the user.
        /// </summary>
        public string FullName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the email address of the user.
        /// </summary>
        public string Email { get; set; } = string.Empty;
    }
}