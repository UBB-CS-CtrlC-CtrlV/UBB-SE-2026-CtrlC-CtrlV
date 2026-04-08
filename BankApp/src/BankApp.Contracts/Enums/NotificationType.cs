namespace BankApp.Contracts.Enums;

/// <summary>
/// Represents the category of a user notification.
/// </summary>
public enum NotificationType
{
    /// <summary>
    /// A payment was made from the account.
    /// </summary>
    Payment,

    /// <summary>
    /// Funds were transferred out of the account.
    /// </summary>
    OutboundTransfer,

    /// <summary>
    /// Funds were received into the account.
    /// </summary>
    InboundTransfer,

    /// <summary>
    /// The account balance has dropped below a configured threshold.
    /// </summary>
    LowBalance,

    /// <summary>
    /// A payment is due soon on the account.
    /// </summary>
    DuePayment,

    /// <summary>
    /// Unusual activity has been detected on the account.
    /// </summary>
    SuspiciousActivity,
}
