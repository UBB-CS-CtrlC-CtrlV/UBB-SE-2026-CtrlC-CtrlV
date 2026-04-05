namespace BankApp.Core.Enums
{
    /// <summary>
    /// Represents the possible states of the login flow.
    /// </summary>
    public enum LoginState
    {
        /// <summary>
        /// No login attempt is in progress. The form is idle and awaiting user input.
        /// </summary>
        Idle,

        /// <summary>
        /// A login request is in progress. The form should be disabled and a loading indicator shown.
        /// </summary>
        Loading,

        /// <summary>
        /// Authentication succeeded. The user should be navigated to the main application.
        /// </summary>
        Success,

        /// <summary>
        /// The server requires a two-factor authentication code before granting access.
        /// </summary>
        Require2Fa,

        /// <summary>
        /// The provided email or password did not match any account.
        /// </summary>
        InvalidCredentials,

        /// <summary>
        /// The account has been temporarily locked due to too many failed login attempts.
        /// </summary>
        AccountLocked,

        /// <summary>
        /// An unexpected error occurred during login. The user should be prompted to try again.
        /// </summary>
        Error,

        /// <summary>
        /// The application is not properly configured and cannot connect to the server.
        /// The login form should be permanently disabled until the configuration is fixed.
        /// </summary>
        ServerNotConfigured,
    }
}
