using BankApp.Contracts.Enums;

namespace BankApp.Contracts.Entities;

/// <summary>
/// Represents a payment card linked to a bank account.
/// </summary>
public class Card
{
    /// <summary>
    /// Gets or sets the unique identifier for the card.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the account this card belongs to.
    /// </summary>
    public int AccountId { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the user who owns this card.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Gets or sets the card number.
    /// </summary>
    public string CardNumber { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the cardholder.
    /// </summary>
    public string CardholderName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the expiry date of the card.
    /// </summary>
    public DateTime ExpiryDate { get; set; }

    /// <summary>
    /// Gets or sets the card verification value.
    /// </summary>
    public string CVV { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the type of the card.
    /// </summary>
    public CardType CardType { get; set; }

    /// <summary>
    /// Gets or sets the brand of the card (e.g. Visa, Mastercard).
    /// </summary>
    public string? CardBrand { get; set; }

    /// <summary>
    /// Gets or sets the status of the card.
    /// </summary>
    public CardStatus Status { get; set; }

    /// <summary>
    /// Gets or sets the daily transaction limit.
    /// </summary>
    public decimal? DailyTransactionLimit { get; set; }

    /// <summary>
    /// Gets or sets the monthly spending cap.
    /// </summary>
    public decimal? MonthlySpendingCap { get; set; }

    /// <summary>
    /// Gets or sets the ATM withdrawal limit.
    /// </summary>
    public decimal? AtmWithdrawalLimit { get; set; }

    /// <summary>
    /// Gets or sets the contactless payment limit.
    /// </summary>
    public decimal? ContactlessLimit { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether contactless payments are enabled.
    /// </summary>
    public bool IsContactlessEnabled { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether online payments are enabled.
    /// </summary>
    public bool IsOnlineEnabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the display sort order for the card.
    /// </summary>
    public int SortOrder { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the card was cancelled, if applicable.
    /// </summary>
    public DateTime? CancelledAt { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the card was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }
}