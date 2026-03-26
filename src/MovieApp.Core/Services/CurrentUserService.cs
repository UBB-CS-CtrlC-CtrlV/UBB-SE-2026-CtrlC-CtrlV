using MovieApp.Core.Models;
using MovieApp.Core.Repositories;

namespace MovieApp.Core.Services;

/// <summary>
/// Resolves and caches the bootstrap user used by the application shell.
/// </summary>
public sealed class CurrentUserService : ICurrentUserService
{
    private readonly IUserRepository _userRepository;
    private readonly BootstrapUserOptions _bootstrapUserOptions;
    private User? _currentUser;

    /// <summary>
    /// Creates the service with the user repository and bootstrap identity configuration.
    /// </summary>
    public CurrentUserService(IUserRepository userRepository, BootstrapUserOptions bootstrapUserOptions)
    {
        _userRepository = userRepository;
        _bootstrapUserOptions = bootstrapUserOptions;
    }

    /// <summary>
    /// Gets the initialized current user.
    /// </summary>
    public User CurrentUser =>
        _currentUser ?? throw new InvalidOperationException("The current user has not been initialized.");

    /// <summary>
    /// Loads the configured bootstrap user once and caches the result.
    /// </summary>
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        if (_currentUser is not null)
        {
            return;
        }

        _currentUser = await _userRepository.FindByAuthIdentityAsync(
            _bootstrapUserOptions.AuthProvider,
            _bootstrapUserOptions.AuthSubject,
            cancellationToken);

        if (_currentUser is null)
        {
            throw new InvalidOperationException(
                $"The configured bootstrap user '{_bootstrapUserOptions.AuthProvider}:{_bootstrapUserOptions.AuthSubject}' could not be found.");
        }
    }
}
