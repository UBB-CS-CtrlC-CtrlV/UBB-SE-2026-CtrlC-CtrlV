namespace BankApp.Server.Controllers
{
    /// <summary>
    /// Data transfer object used for reset token verification requests.
    /// </summary>
    public class VerifyTokenDto
    {
        /// <summary>
        /// Gets or sets the reset token to be verified.
        /// </summary>
        public string Token { get; set; } = string.Empty;
    }
}