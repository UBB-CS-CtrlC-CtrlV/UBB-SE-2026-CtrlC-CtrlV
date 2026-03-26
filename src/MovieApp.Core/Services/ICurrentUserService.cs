using MovieApp.Core.Models;

namespace MovieApp.Core.Services;

/// <summary>
/// Exposes the currently authenticated application user.
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// Gets the initialized current user.
    /// </summary>
    User CurrentUser { get; }

    /// <summary>
    /// Initializes the current-user context.
    /// </summary>
    Task InitializeAsync(CancellationToken cancellationToken = default);
}
