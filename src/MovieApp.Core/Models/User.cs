namespace MovieApp.Core.Models;

/// <summary>
/// Represents an authenticated application user.
/// </summary>
public sealed class User
{
    /// <summary>
    /// Gets the internal user identifier.
    /// </summary>
    public required int Id { get; init; }

    /// <summary>
    /// Gets the external authentication provider name.
    /// </summary>
    public required string AuthProvider { get; init; }

    /// <summary>
    /// Gets the external authentication subject identifier.
    /// </summary>
    public required string AuthSubject { get; init; }

    /// <summary>
    /// Gets the username shown in the application.
    /// </summary>
    public required string Username { get; init; }

    /// <summary>
    /// Gets the stable composite identifier used for seeded-user lookup.
    /// </summary>
    public string StableId => $"{AuthProvider}:{AuthSubject}";
}
