namespace BankApp.Core.Enums
{
    /// <summary>
    /// TODO: add docs.
    /// </summary>
    public enum LoginState
    {
        /// <summary>
        /// TODO: add docs.
        /// </summary>
        Idle,

        /// <summary>
        /// TODO: add docs.
        /// </summary>
        Loading,

        /// <summary>
        /// TODO: add docs.
        /// </summary>
        Success,

        /// <summary>
        /// TODO: add docs.
        /// </summary>
        Require2Fa,

        /// <summary>
        /// TODO: add docs.
        /// </summary>
        InvalidCredentials,

        /// <summary>
        /// TODO: add docs.
        /// </summary>
        AccountLocked,

        /// <summary>
        /// TODO: add docs.
        /// </summary>
        Error,

        /// <summary>
        /// TODO: add docs.
        /// </summary>
        ServerNotConfigured,
    }
}
