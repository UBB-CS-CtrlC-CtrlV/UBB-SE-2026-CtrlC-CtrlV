namespace MovieApp.Core.Services;

/// <summary>
/// Configures the bootstrap identity that is resolved as the app's current user.
/// </summary>
public sealed class BootstrapUserOptions
{
    /// <summary>
    /// Gets the external authentication provider identifier.
    /// </summary>
    public required string AuthProvider { get; init; }

    /// <summary>
    /// Gets the external authentication subject identifier.
    /// </summary>
    public required string AuthSubject { get; init; }
}
