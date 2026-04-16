using BankApp.Client.Utilities;
using BankApp.Client.ViewModels;
using BankApp.Client.Enums;
using BankApp.Contracts.DTOs.Auth;
using ErrorOr;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace BankApp.Desktop.Tests.ViewModels;

public class LoginViewModelTests
{
    private readonly ApiClient apiClient;
    private readonly IConfiguration configuration;

    public LoginViewModelTests()
    {
        var configBuilder = new ConfigurationBuilder();
        configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
        {
            { "ApiBaseUrl", "http://localhost" },
            { "OAuth:Google:Authority", "https://accounts.google.com" },
            { "OAuth:Google:ClientId", "client-id" },
            { "OAuth:Google:ClientSecret", "client-secret" },
            { "OAuth:Google:RedirectUri", "http://localhost:5000/callback" }
        });

        this.configuration = configBuilder.Build();
        this.apiClient = Substitute.ForPartsOf<ApiClient>(this.configuration, NullLogger<ApiClient>.Instance);
    }

    [Fact]
    public void CanLogin_WhenValid_ReturnsTrue()
    {
        // Arrange
        var vm = new LoginViewModel(this.apiClient, this.configuration, NullLogger<LoginViewModel>.Instance);

        // Act & Assert
        vm.CanLogin("test@test.com", "password").Should().BeTrue();
    }

    [Fact]
    public void CanLogin_WhenInvalid_ReturnsFalse()
    {
        // Arrange
        var vm = new LoginViewModel(this.apiClient, this.configuration, NullLogger<LoginViewModel>.Instance);

        // Act & Assert
        vm.CanLogin(string.Empty, "password").Should().BeFalse();
        vm.CanLogin("test@test.com", string.Empty).Should().BeFalse();
        vm.CanLogin(string.Empty, string.Empty).Should().BeFalse();
        vm.CanLogin(" ", " ").Should().BeFalse();
    }

    [Fact]
    public async Task Login_WhenSuccess_SetsStateToSuccessAndSetsToken()
    {
        // Arrange
        var vm = new LoginViewModel(this.apiClient, this.configuration, NullLogger<LoginViewModel>.Instance);
        var response = new LoginSuccessResponse { Token = "test-token", UserId = 1, Requires2FA = false };

        this.apiClient.PostAsync<LoginRequest, LoginSuccessResponse>(Arg.Any<string>(), Arg.Any<LoginRequest>())
            .Returns(Task.FromResult<ErrorOr<LoginSuccessResponse>>(response));

        // Act
        await vm.Login("test@test.com", "password");

        // Assert
        vm.State.Value.Should().Be(LoginState.Success);
        this.apiClient.CurrentUserId.Should().Be(1);
    }

    [Fact]
    public async Task Login_WhenRequires2FA_SetsStateToRequire2Fa()
    {
        // Arrange
        var vm = new LoginViewModel(this.apiClient, this.configuration, NullLogger<LoginViewModel>.Instance);
        var response = new LoginSuccessResponse { UserId = 1, Requires2FA = true };

        this.apiClient.PostAsync<LoginRequest, LoginSuccessResponse>(Arg.Any<string>(), Arg.Any<LoginRequest>())
            .Returns(Task.FromResult<ErrorOr<LoginSuccessResponse>>(response));

        // Act
        await vm.Login("test@test.com", "password");

        // Assert
        vm.State.Value.Should().Be(LoginState.Require2Fa);
        this.apiClient.CurrentUserId.Should().Be(1);
    }

    [Fact]
    public async Task Login_WhenUnauthorized_SetsStateToInvalidCredentials()
    {
        // Arrange
        var vm = new LoginViewModel(this.apiClient, this.configuration, NullLogger<LoginViewModel>.Instance);

        this.apiClient.PostAsync<LoginRequest, LoginSuccessResponse>(Arg.Any<string>(), Arg.Any<LoginRequest>())
            .Returns(Task.FromResult<ErrorOr<LoginSuccessResponse>>(Error.Validation("error", "msg")));

        // Act
        await vm.Login("test@test.com", "password");

        // Assert
        vm.State.Value.Should().Be(LoginState.Error);
    }
}
