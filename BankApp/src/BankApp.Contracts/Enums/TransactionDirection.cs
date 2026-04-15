namespace BankApp.Contracts.Enums;

/// <summary>
/// Represents the direction of a transaction, inbound or outbound.
/// </summary>
public enum TransactionDirection
{
    /// <summary>
    /// The transaction is inbound. (Receiving)
    /// </summary>
    In,

    /// <summary>
    /// The transaction is outbound. (Sending)
    /// </summary>
    Out
}