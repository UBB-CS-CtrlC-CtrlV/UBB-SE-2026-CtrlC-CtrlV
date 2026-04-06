namespace BankApp.Core.Entities
{
    /// <summary>
    /// Represents a bank account belonging to a user.
    /// </summary>
    public class Account
    {
        /// <summary>
        /// Gets or sets the unique identifier for the account.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the user who owns this account.
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Gets or sets the display name of the account.
        /// </summary>
        public string? AccountName { get; set; }

        /// <summary>
        /// Gets or sets the International Bank Account Number.
        /// </summary>
        public string IBAN { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the currency code for the account.
        /// </summary>
        public string Currency { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the current balance of the account.
        /// </summary>
        public decimal Balance { get; set; }

        /// <summary>
        /// Gets or sets the type of the account.
        /// </summary>
        public string AccountType { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the status of the account.
        /// </summary>
        public string Status { get; set; } = "Active";

        /// <summary>
        /// Gets or sets the date and time when the account was created.
        /// </summary>
        public DateTime CreatedAt { get; set; }
    }
}
