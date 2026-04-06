using BankApp.Core.Enums;

namespace BankApp.Core.DTOs.Profile
{
    /// <summary>
    /// Represents a request to enable two-factor authentication.
    /// </summary>
    public class Enable2FARequest
    {
        /// <summary>
        /// Gets or sets the two-factor authentication method to enable.
        /// </summary>
        public TwoFactorMethod Method { get; set; }
    }
}
