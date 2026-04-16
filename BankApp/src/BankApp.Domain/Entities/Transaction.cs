using BankApp.Domain.Enums;

namespace BankApp.Domain.Entities;

/// <summary>
/// Represents a financial transaction on an account.
/// </summary>
public class Transaction
{
    /// <summary>
    /// Gets or sets the unique identifier for the transaction.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the account this transaction belongs to.
    /// </summary>
    public int AccountId { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the card used for this transaction, if any.
    /// </summary>
    public int? CardId { get; set; }

    /// <summary>
    /// Gets or sets the unique reference code for the transaction.
    /// </summary>
    public string TransactionRef { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the direction of the transaction as a <see cref="TransactionDirection"/>.
    /// </summary>
    public TransactionDirection Direction { get; set; }

    /// <summary>
    /// Gets or sets the amount of the transaction.
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Gets or sets the currency code for the transaction.
    /// </summary>
    public string Currency { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the account balance after this transaction.
    /// </summary>
    public decimal BalanceAfter { get; set; }

    /// <summary>
    /// Gets or sets the name of the counterparty in the transaction.
    /// </summary>
    public string? CounterpartyName { get; set; }

    /// <summary>
    /// Gets or sets the IBAN of the counterparty.
    /// </summary>
    public string? CounterpartyIBAN { get; set; }

    /// <summary>
    /// Gets or sets the merchant name for card transactions.
    /// </summary>
    public string? MerchantName { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the transaction category.
    /// </summary>
    public int? CategoryId { get; set; }

    /// <summary>
    /// Gets or sets the description of the transaction.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the fee charged for the transaction.
    /// </summary>
    public decimal Fee { get; set; }

    /// <summary>
    /// Gets or sets the exchange rate applied to the transaction, if applicable.
    /// </summary>
    public decimal? ExchangeRate { get; set; }

    /// <summary>
    /// Gets or sets the status of the transaction as a <see cref="TransactionStatus"/>.
    /// </summary>
    public TransactionStatus Status { get; set; }

    /// <summary>
    /// Gets or sets the type of entity related to this transaction.
    /// </summary>
    public string? RelatedEntityType { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the entity related to this transaction.
    /// </summary>
    public int? RelatedEntityId { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the transaction was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }
}