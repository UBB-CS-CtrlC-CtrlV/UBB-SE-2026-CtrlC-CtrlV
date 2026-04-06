namespace BankApp.Core.DTOs.Auth
{
    /// <summary>
    /// Represents the response returned after a registration attempt.
    /// </summary>
    public class RegisterResponse
    {
        /// <summary>
        /// Gets or sets a value indicating whether the registration was successful.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Gets or sets the error message, if registration failed.
        /// </summary>
        public string? Error { get; set; }
    }
}
