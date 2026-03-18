using MovieApp.Models;

namespace MovieApp.Services;

public sealed class CurrentUserService : ICurrentUserService
{
    private readonly IUserRepository _userRepository;
    private readonly BootstrapUserOptions _bootstrapUserOptions;
    private User? _currentUser;

    public CurrentUserService(IUserRepository userRepository, BootstrapUserOptions bootstrapUserOptions)
    {
        _userRepository = userRepository;
        _bootstrapUserOptions = bootstrapUserOptions;
    }

    public User CurrentUser =>
        _currentUser ?? throw new InvalidOperationException("The current user has not been initialized.");

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
                $"The seeded dummy user '{_bootstrapUserOptions.AuthProvider}:{_bootstrapUserOptions.AuthSubject}' could not be found.");
        }
    }
}
