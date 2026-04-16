using BankApp.Client.Enums;
using BankApp.Client.Utilities;
using BankApp.Client.ViewModels;
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
        Assert.False(sut.CanLogin(string.Empty, string.Empty));
    }

    [Fact]
    public void CanLogin_EmptyEmail_ReturnsFalse()
    {
        var sut = CreateViewModel();
        Assert.False(sut.CanLogin(string.Empty, "Password1!"));
    }

    [Fact]
    public void CanLogin_EmptyPassword_ReturnsFalse()
    {
        var sut = CreateViewModel();
        Assert.False(sut.CanLogin("user@test.com", string.Empty));
    }

    [Fact]
    public void CanLogin_ValidInputs_ReturnsTrue()
    {
        var sut = CreateViewModel();
        Assert.True(sut.CanLogin("user@test.com", "Password1!"));
    }

    [Fact]
    public void Constructor_MissingApiBaseUrl_SetsServerNotConfiguredState()
    {
        IConfiguration config = new ConfigurationBuilder().Build();

        var sut = new LoginViewModel(
            new ApiClient(config, NullLogger<ApiClient>.Instance),
            config,
            NullLogger<LoginViewModel>.Instance);

        Assert.Equal(LoginState.ServerNotConfigured, sut.State.Value);
    }

    [Fact]
    public void Constructor_ValidConfig_SetsIdleState()
    {
        var sut = CreateViewModel();
        Assert.Equal(LoginState.Idle, sut.State.Value);
    }
}