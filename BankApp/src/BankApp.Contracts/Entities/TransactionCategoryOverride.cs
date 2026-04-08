namespace BankApp.Contracts.Entities;

/// <summary>
/// Represents a user override of the default category for a transaction.
/// </summary>
public class TransactionCategoryOverride
{
    /// <summary>
    /// Gets or sets the unique identifier for the override.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the transaction being overridden.
    /// </summary>
    public int TransactionId { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the user who created this override.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the new category assigned by the user.
    /// </summary>
    public int CategoryId { get; set; }
}