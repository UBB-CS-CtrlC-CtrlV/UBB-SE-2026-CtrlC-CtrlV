using System.Threading.Tasks;
using MovieApp.Core.Models;
using MovieApp.Core.Services;

namespace MovieApp.Ui.Services;

public class FakeCurrentUserService : ICurrentUserService
{
    public User CurrentUser { get; private set; } = new User { Id = 1, Username = "DemoUser", AuthProvider = "dummy", AuthSubject = "123" };

    public Task InitializeAsync(System.Threading.CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public void SetCurrentUser(User? user)
    {
        CurrentUser = user!;
    }
}
