using BankApp.Client.Utilities;
using BankApp.Client.ViewModels;
using BankApp.Client.Enums;
using BankApp.Contracts.DTOs.Auth;
using ErrorOr;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
//TODO: replace NSubstitute with Moq

namespace BankApp.Desktop.Tests.ViewModels;

public class RegisterViewModelTests
{
    private readonly ApiClient apiClient;
    private readonly IConfiguration configuration;

    public RegisterViewModelTests()
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
    public async Task Register_WhenEmptyFields_SetsErrorState()
    {
        // Arrange
        var vm = new RegisterViewModel(this.apiClient, this.configuration, NullLogger<RegisterViewModel>.Instance);

        // Act
        await vm.Register(string.Empty, "pass", "pass", "Name");

        // Assert
        vm.State.Value.Should().Be(RegisterState.Error);
    }

    [Fact]
    public async Task Register_WhenPasswordMismatch_SetsPasswordMismatchState()
    {
        // Arrange
        var vm = new RegisterViewModel(this.apiClient, this.configuration, NullLogger<RegisterViewModel>.Instance);

        // Act
        await vm.Register("test@test.com", "Password123!", "Password123", "Name");

        // Assert
        vm.State.Value.Should().Be(RegisterState.PasswordMismatch);
    }

    [Fact]
    public async Task Register_WhenWeakPassword_SetsWeakPasswordState()
    {
        // Arrange
        var vm = new RegisterViewModel(this.apiClient, this.configuration, NullLogger<RegisterViewModel>.Instance);

        // Act
        await vm.Register("test@test.com", "weak", "weak", "Name");

        // Assert
        vm.State.Value.Should().Be(RegisterState.WeakPassword);
    }

    [Fact]
    public async Task Register_WhenValid_SetsSuccessState()
    {
        // Arrange
        var vm = new RegisterViewModel(this.apiClient, this.configuration, NullLogger<RegisterViewModel>.Instance);

        this.apiClient.PostAsync<RegisterRequest>(Arg.Any<string>(), Arg.Any<RegisterRequest>())
            .Returns(Task.FromResult<ErrorOr<Success>>(Result.Success));

        // Act
        await vm.Register("test@test.com", "StrongP@ss1", "StrongP@ss1", "Name");

        // Assert
        vm.State.Value.Should().Be(RegisterState.Success);
    }

    [Fact]
    public async Task Register_WhenEmailConflicts_SetsEmailAlreadyExistsState()
    {
        // Arrange
        var vm = new RegisterViewModel(this.apiClient, this.configuration, NullLogger<RegisterViewModel>.Instance);

        this.apiClient.PostAsync<RegisterRequest>(Arg.Any<string>(), Arg.Any<RegisterRequest>())
            .Returns(Task.FromResult<ErrorOr<Success>>(Error.Conflict("Conflict", "Conflict")));

        // Act
        await vm.Register("test@test.com", "StrongP@ss1", "StrongP@ss1", "Name");

        // Assert
        vm.State.Value.Should().Be(RegisterState.EmailAlreadyExists);
    }
}
