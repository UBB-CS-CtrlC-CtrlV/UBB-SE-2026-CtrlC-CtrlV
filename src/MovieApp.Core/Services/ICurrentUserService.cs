using MovieApp.Core.Models;

namespace MovieApp.Core.Services;

public interface ICurrentUserService
{
    User CurrentUser { get; }

    Task InitializeAsync(CancellationToken cancellationToken = default);
}
