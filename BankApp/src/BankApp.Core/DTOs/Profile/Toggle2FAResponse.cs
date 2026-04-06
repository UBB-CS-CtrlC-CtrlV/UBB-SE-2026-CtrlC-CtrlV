namespace BankApp.Core.DTOs.Profile
{
    /// <summary>
    /// Represents the response returned after toggling two-factor authentication.
    /// </summary>
    public class Toggle2FAResponse
    {
        /// <summary>
        /// Gets or sets a value indicating whether the toggle was successful.
        /// </summary>
        public bool Success { get; set; }
    }
}
