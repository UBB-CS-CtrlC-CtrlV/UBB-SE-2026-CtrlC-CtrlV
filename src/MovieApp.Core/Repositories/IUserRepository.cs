using MovieApp.Core.Models;

namespace MovieApp.Core.Repositories;

/// <summary>
/// Loads users from persistent storage.
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Finds a user by external authentication identity.
    /// </summary>
    Task<User?> FindByAuthIdentityAsync(string authProvider, string authSubject, CancellationToken cancellationToken = default);
}
