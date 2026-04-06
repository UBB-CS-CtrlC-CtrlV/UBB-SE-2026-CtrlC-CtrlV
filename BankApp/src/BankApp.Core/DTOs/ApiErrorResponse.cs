namespace BankApp.Core.DTOs
{
    /// <summary>
    /// Represents an error response returned by the API.
    /// </summary>
    public class ApiErrorResponse
    {
        /// <summary>
        /// Gets or sets the error message.
        /// </summary>
        public string Message { get; set; } = string.Empty;
    }
}
