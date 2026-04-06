namespace BankApp.Core.DTOs.Profile
{
    /// <summary>
    /// Represents the response returned after a profile update attempt.
    /// </summary>
    public class UpdateProfileResponse
    {
        /// <summary>
        /// Gets or sets a value indicating whether the update was successful.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Gets or sets the message describing the result.
        /// </summary>
        public string? Message { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateProfileResponse"/> class.
        /// </summary>
        public UpdateProfileResponse()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateProfileResponse"/> class.
        /// </summary>
        /// <param name="success">Whether the update was successful.</param>
        /// <param name="message">A message describing the result.</param>
        public UpdateProfileResponse(bool success, string? message)
        {
            Success = success;
            Message = message;
        }
    }
}
