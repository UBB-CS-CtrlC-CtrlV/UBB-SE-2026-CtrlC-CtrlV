using BankApp.Client.Enums;
using BankApp.Client.Utilities;
using BankApp.Client.ViewModels;
using FluentAssertions;
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
        sut.State.Value.Should().Be(RegisterState.Error);
    }

    [Fact]
    public async Task Register_InvalidEmail_SetsInvalidEmailState()
    {
        var sut = CreateViewModel();
        await sut.Register("notanemail", "Password1!", "Password1!", "John Doe");
        sut.State.Value.Should().Be(RegisterState.InvalidEmail);
    }

    [Fact]
    public async Task Register_WeakPassword_SetsWeakPasswordState()
    {
        var sut = CreateViewModel();
        await sut.Register("user@test.com", "weak", "weak", "John Doe");
        sut.State.Value.Should().Be(RegisterState.WeakPassword);
    }

    [Fact]
    public async Task Register_PasswordMismatch_SetsPasswordMismatchState()
    {
        var sut = CreateViewModel();
        await sut.Register("user@test.com", "Password1!", "Password2!", "John Doe");
        sut.State.Value.Should().Be(RegisterState.PasswordMismatch);
    }

    [Fact]
    public void Constructor_StartsInIdleState()
    {
        var sut = CreateViewModel();
        sut.State.Value.Should().Be(RegisterState.Idle);
    }
}