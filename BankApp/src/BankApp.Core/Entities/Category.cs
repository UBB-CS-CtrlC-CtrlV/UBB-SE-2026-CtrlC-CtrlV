namespace BankApp.Core.Entities
{
    /// <summary>
    /// Represents a transaction category.
    /// </summary>
    public class Category
    {
        /// <summary>
        /// Gets or sets the unique identifier for the category.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the category.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the icon identifier for the category.
        /// </summary>
        public string? Icon { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this is a system-defined category.
        /// </summary>
        public bool IsSystem { get; set; } = true;
    }
}
