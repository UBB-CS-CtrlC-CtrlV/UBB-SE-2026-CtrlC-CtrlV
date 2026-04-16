using BankApp.Client.Enums;
using BankApp.Client.Utilities;
using BankApp.Client.ViewModels;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;

namespace BankApp.Client.Tests;

public class LoginViewModelTests
{
    private static LoginViewModel CreateViewModel()
    {
        IConfiguration config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ApiBaseUrl"] = "http://localhost",
            })
            .Build();

        return new LoginViewModel(
            new ApiClient(config, NullLogger<ApiClient>.Instance),
            config,
            NullLogger<LoginViewModel>.Instance);
    }

    [Fact]
    public void CanLogin_BothFieldsEmpty_ReturnsFalse()
    {
        var sut = CreateViewModel();
        sut.CanLogin(string.Empty, string.Empty).Should().BeFalse();
    }

    [Fact]
    public void CanLogin_EmptyEmail_ReturnsFalse()
    {
        var sut = CreateViewModel();
        sut.CanLogin(string.Empty, "Password1!").Should().BeFalse();
    }

    [Fact]
    public void CanLogin_EmptyPassword_ReturnsFalse()
    {
        var sut = CreateViewModel();
        sut.CanLogin("user@test.com", string.Empty).Should().BeFalse();
    }

    [Fact]
    public void CanLogin_ValidInputs_ReturnsTrue()
    {
        var sut = CreateViewModel();
        sut.CanLogin("user@test.com", "Password1!").Should().BeTrue();
    }

    [Fact]
    public void Constructor_MissingApiBaseUrl_SetsServerNotConfiguredState()
    {
        IConfiguration config = new ConfigurationBuilder().Build();

        var sut = new LoginViewModel(
            new ApiClient(config, NullLogger<ApiClient>.Instance),
            config,
            NullLogger<LoginViewModel>.Instance);

        sut.State.Value.Should().Be(LoginState.ServerNotConfigured);
    }

    [Fact]
    public void Constructor_ValidConfig_SetsIdleState()
    {
        var sut = CreateViewModel();
        sut.State.Value.Should().Be(LoginState.Idle);
    }
}