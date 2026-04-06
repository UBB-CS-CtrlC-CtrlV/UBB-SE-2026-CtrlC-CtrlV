namespace BankApp.Core.DTOs.Profile
{
    /// <summary>
    /// Represents a request to change the user's password.
    /// </summary>
    public class ChangePasswordRequest
    {
        /// <summary>
        /// Gets or sets the identifier of the user changing their password.
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Gets or sets the current password for verification.
        /// </summary>
        public string CurrentPassword { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the new password to set.
        /// </summary>
        public string NewPassword { get; set; } = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChangePasswordRequest"/> class.
        /// </summary>
        public ChangePasswordRequest()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChangePasswordRequest"/> class.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="currentPassword">The current password.</param>
        /// <param name="newPassword">The new password.</param>
        public ChangePasswordRequest(int userId, string currentPassword, string newPassword)
        {
            UserId = userId;
            CurrentPassword = currentPassword;
            NewPassword = newPassword;
        }
    }
}
