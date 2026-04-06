namespace BankApp.Core.DTOs.Auth
{
    /// <summary>
    /// Represents the response returned after a login attempt.
    /// </summary>
    public class LoginResponse
    {
        /// <summary>
        /// Gets or sets a value indicating whether the login was successful.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Gets or sets the authentication token, if login was successful.
        /// </summary>
        public string? Token { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether two-factor authentication is required.
        /// </summary>
        public bool Requires2FA { get; set; }

        /// <summary>
        /// Gets or sets the user identifier, if login was successful.
        /// </summary>
        public int? UserId { get; set; }

        /// <summary>
        /// Gets or sets the error message, if login failed.
        /// </summary>
        public string? Error { get; set; }
    }
}
