using BankApp.Client.Enums;
using BankApp.Client.Utilities;
using BankApp.Client.ViewModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;

namespace BankApp.Client.Tests;

public class RegisterViewModelTests
{
    private static RegisterViewModel CreateViewModel()
    {
        IConfiguration config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ApiBaseUrl"] = "http://localhost",
            })
            .Build();

        return new RegisterViewModel(
            new ApiClient(config, NullLogger<ApiClient>.Instance),
            config,
            NullLogger<RegisterViewModel>.Instance);
    }

    [Fact]
    public async Task Register_EmptyFields_SetsErrorState()
    {
        var sut = CreateViewModel();
        await sut.Register(string.Empty, string.Empty, string.Empty, string.Empty);
        Assert.Equal(RegisterState.Error, sut.State.Value);
    }

    [Fact]
    public async Task Register_InvalidEmail_SetsInvalidEmailState()
    {
        var sut = CreateViewModel();
        await sut.Register("notanemail", "Password1!", "Password1!", "John Doe");
        Assert.Equal(RegisterState.InvalidEmail, sut.State.Value);
    }

    [Fact]
    public async Task Register_WeakPassword_SetsWeakPasswordState()
    {
        var sut = CreateViewModel();
        await sut.Register("user@test.com", "weak", "weak", "John Doe");
        Assert.Equal(RegisterState.WeakPassword, sut.State.Value);
    }

    [Fact]
    public async Task Register_PasswordMismatch_SetsPasswordMismatchState()
    {
        var sut = CreateViewModel();
        await sut.Register("user@test.com", "Password1!", "Password2!", "John Doe");
        Assert.Equal(RegisterState.PasswordMismatch, sut.State.Value);
    }

    [Fact]
    public void Constructor_StartsInIdleState()
    {
        var sut = CreateViewModel();
        Assert.Equal(RegisterState.Idle, sut.State.Value);
    }
}