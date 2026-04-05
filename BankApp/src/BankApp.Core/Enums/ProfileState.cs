namespace BankApp.Core.Enums
{
    /// <summary>
    /// Represents the possible states of the profile management flow.
    /// </summary>
    public enum ProfileState
    {
        /// <summary>
        /// No operation is in progress. The form is idle and awaiting user input.
        /// </summary>
        Idle,

        /// <summary>
        /// A profile operation is in progress.
        /// </summary>
        Loading,

        /// <summary>
        /// Profile data was loaded successfully and is ready to display.
        /// </summary>
        Success,

        /// <summary>
        /// A profile field update (e.g. name or email) completed successfully.
        /// </summary>
        UpdateSuccess,

        /// <summary>
        /// A password change completed successfully.
        /// </summary>
        PasswordChanged,

        /// <summary>
        /// An unexpected error occurred during a profile operation.
        /// </summary>
        Error,
    }
}
