using MovieApp.Core.Models;
using MovieApp.Core.Repositories;
using MovieApp.Core.Services;
using Xunit;

namespace MovieApp.Core.Tests;

public sealed class CurrentUserServiceTests
{
    [Fact]
    public void CurrentUser_ThrowsBeforeInitializeAsync()
    {
        var service = CreateService(returnedUser: null);

        Assert.Throws<InvalidOperationException>(() => _ = service.CurrentUser);
    }

    [Fact]
    public async Task InitializeAsync_LoadsSeededUserOnceAndCachesIt()
    {
        var expectedUser = new User
        {
            Id = 7,
            AuthProvider = "seed",
            AuthSubject = "dummy",
            Username = "alice",
        };
        var repository = new StubUserRepository(expectedUser);
        var service = CreateService(repository);

        await service.InitializeAsync();
        await service.InitializeAsync();

        Assert.Equal(expectedUser, service.CurrentUser);
        Assert.Equal(1, repository.FindCalls);
    }

    [Fact]
    public async Task InitializeAsync_ThrowsWhenSeededUserDoesNotExist()
    {
        var service = CreateService(returnedUser: null);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => service.InitializeAsync());

        Assert.Contains("seed:dummy", exception.Message);
    }

    private static CurrentUserService CreateService(User? returnedUser)
    {
        return CreateService(new StubUserRepository(returnedUser));
    }

    private static CurrentUserService CreateService(StubUserRepository repository)
    {
        return new CurrentUserService(
            repository,
            new BootstrapUserOptions
            {
                AuthProvider = "seed",
                AuthSubject = "dummy",
            });
    }

    private sealed class StubUserRepository(User? returnedUser) : IUserRepository
    {
        public int FindCalls { get; private set; }

        public Task<User?> FindByAuthIdentityAsync(string authProvider, string authSubject, CancellationToken cancellationToken = default)
        {
            FindCalls++;
            return Task.FromResult(returnedUser);
        }
    }
}
