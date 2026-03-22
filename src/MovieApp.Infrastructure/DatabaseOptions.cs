namespace MovieApp.Infrastructure;

/// <summary>
/// Shared database configuration used by SQL-backed repositories.
/// </summary>
public sealed class DatabaseOptions
{
    /// <summary>
    /// SQL Server connection string for the MovieApp database.
    /// </summary>
    public required string ConnectionString { get; init; }
}
