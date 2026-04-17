// <copyright file="RegisterViewModelTests.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

using BankApp.Application.DataTransferObjects.Auth;
using BankApp.Desktop.Enums;
using BankApp.Desktop.Utilities;
using BankApp.Desktop.ViewModels;
using ErrorOr;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace BankApp.Desktop.Tests.ViewModels;

/// <summary>
/// Tests for <see cref="RegisterViewModel"/>.
/// </summary>
public class RegisterViewModelTests
{
    private readonly Mock<IApiClient> apiClient = new Mock<IApiClient>();
    private readonly IConfiguration configuration;

    /// <summary>
    /// Initializes a new instance of the <see cref="RegisterViewModelTests"/> class.
    /// </summary>
    public RegisterViewModelTests()
    {
        var configBuilder = new ConfigurationBuilder();
        configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
        {
            { "ApiBaseUrl", "http://localhost" },
            { "OAuth:Google:Authority", "https://accounts.google.com" },
            { "OAuth:Google:ClientId", "client-id" },
            { "OAuth:Google:ClientSecret", "client-secret" },
            { "OAuth:Google:RedirectUri", "http://localhost:5000/callback" },
        });

        this.configuration = configBuilder.Build();
    }

    /// <summary>
    /// When any required field is empty, state transitions to <see cref="RegisterState.Error"/>.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task Register_WhenEmptyFields_SetsErrorState()
    {
        // Arrange
        var viewModel = new RegisterViewModel(this.apiClient.Object, this.configuration, NullLogger<RegisterViewModel>.Instance);

        // Act
        await viewModel.Register(string.Empty, "pass", "pass", "Name");

        // Assert
        viewModel.State.Value.Should().Be(RegisterState.Error);
    }

    /// <summary>
    /// When the password and confirmation do not match, state transitions to
    /// <see cref="RegisterState.PasswordMismatch"/>.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task Register_WhenPasswordMismatch_SetsPasswordMismatchState()
    {
        // Arrange
        var viewModel = new RegisterViewModel(this.apiClient.Object, this.configuration, NullLogger<RegisterViewModel>.Instance);

        // Act
        await viewModel.Register("test@test.com", "Password123!", "Password123", "Name");

        // Assert
        viewModel.State.Value.Should().Be(RegisterState.PasswordMismatch);
    }

    /// <summary>
    /// When the password does not meet complexity requirements, state transitions to
    /// <see cref="RegisterState.WeakPassword"/>.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task Register_WhenWeakPassword_SetsWeakPasswordState()
    {
        // Arrange
        var viewModel = new RegisterViewModel(this.apiClient.Object, this.configuration, NullLogger<RegisterViewModel>.Instance);

        // Act
        await viewModel.Register("test@test.com", "weak", "weak", "Name");

        // Assert
        viewModel.State.Value.Should().Be(RegisterState.WeakPassword);
    }

    /// <summary>
    /// When all inputs are valid and the API succeeds, state transitions to
    /// <see cref="RegisterState.Success"/>.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task Register_WhenValid_SetsSuccessState()
    {
        // Arrange
        var viewModel = new RegisterViewModel(this.apiClient.Object, this.configuration, NullLogger<RegisterViewModel>.Instance);

        this.apiClient
            .Setup(client => client.PostAsync<RegisterRequest>(It.IsAny<string>(), It.IsAny<RegisterRequest>()))
            .ReturnsAsync(Result.Success);

        // Act
        await viewModel.Register("test@test.com", "StrongP@ss1", "StrongP@ss1", "Name");

        // Assert
        viewModel.State.Value.Should().Be(RegisterState.Success);
    }

    /// <summary>
    /// When the API returns a Conflict error, state transitions to
    /// <see cref="RegisterState.EmailAlreadyExists"/>.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task Register_WhenEmailConflicts_SetsEmailAlreadyExistsState()
    {
        // Arrange
        var viewModel = new RegisterViewModel(this.apiClient.Object, this.configuration, NullLogger<RegisterViewModel>.Instance);

        this.apiClient
            .Setup(client => client.PostAsync<RegisterRequest>(It.IsAny<string>(), It.IsAny<RegisterRequest>()))
            .ReturnsAsync(Error.Conflict("Conflict", "Conflict"));

        // Act
        await viewModel.Register("test@test.com", "StrongP@ss1", "StrongP@ss1", "Name");

        // Assert
        viewModel.State.Value.Should().Be(RegisterState.EmailAlreadyExists);
    }
}
