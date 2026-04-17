// <copyright file="ForgotPasswordViewModelTests.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

using BankApp.Desktop.Enums;
using BankApp.Desktop.Utilities;
using BankApp.Desktop.ViewModels;
using FluentAssertions;
using Moq;
using Xunit;

namespace BankApp.Desktop.Tests.ViewModels;

/// <summary>
/// Tests for <see cref="ForgotPasswordViewModel"/>.
/// </summary>
public class ForgotPasswordViewModelTests
{
    private readonly Mock<IPasswordRecoveryManager> recoveryManager = new Mock<IPasswordRecoveryManager>();

    /// <summary>
    /// When the email is empty, state transitions to <see cref="ForgotPasswordState.Error"/>
    /// and a validation message is set without calling the recovery manager.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task ForgotPassword_WhenEmailEmpty_SetsErrorState()
    {
        // Arrange
        var viewModel = new ForgotPasswordViewModel(this.recoveryManager.Object);

        // Act
        await viewModel.ForgotPassword(string.Empty);

        // Assert
        viewModel.State.Value.Should().Be(ForgotPasswordState.Error);
        viewModel.ValidationError.Should().NotBeEmpty();
    }

    /// <summary>
    /// When a valid email is provided, the recovery manager is called and state transitions to
    /// <see cref="ForgotPasswordState.EmailSent"/>.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task ForgotPassword_WhenValidEmail_SetsCodeSentState()
    {
        // Arrange
        var viewModel = new ForgotPasswordViewModel(this.recoveryManager.Object);
        this.recoveryManager
            .Setup(recoveryManagerMock => recoveryManagerMock.RequestCodeAsync(It.IsAny<string>()))
            .ReturnsAsync(ForgotPasswordState.EmailSent);

        // Act
        await viewModel.ForgotPassword("test@bank.com");

        // Assert
        viewModel.State.Value.Should().Be(ForgotPasswordState.EmailSent);
        viewModel.ValidationError.Should().BeEmpty();
    }

    /// <summary>
    /// When both the token and the new password are empty, state transitions to
    /// <see cref="ForgotPasswordState.Error"/> and a validation message is set.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task ResetPassword_WhenFieldsEmpty_SetsErrorState()
    {
        // Arrange
        var viewModel = new ForgotPasswordViewModel(this.recoveryManager.Object);

        // Act
        await viewModel.ResetPassword(string.Empty, string.Empty);

        // Assert
        viewModel.State.Value.Should().Be(ForgotPasswordState.Error);
        viewModel.ValidationError.Should().NotBeEmpty();
    }

    /// <summary>
    /// When the supplied password does not meet complexity requirements, state transitions to
    /// <see cref="ForgotPasswordState.Error"/> and a validation message is set.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task ResetPassword_WhenPasswordTooWeak_SetsErrorState()
    {
        // Arrange
        var viewModel = new ForgotPasswordViewModel(this.recoveryManager.Object);
        this.recoveryManager
            .Setup(recoveryManagerMock => recoveryManagerMock.IsPasswordValid(It.IsAny<string>()))
            .Returns(false);

        // Act
        await viewModel.ResetPassword("weak", "123456");

        // Assert
        viewModel.State.Value.Should().Be(ForgotPasswordState.Error);
        viewModel.ValidationError.Should().NotBeEmpty();
    }

    /// <summary>
    /// When both the token and password are valid, state transitions to
    /// <see cref="ForgotPasswordState.PasswordResetSuccess"/>.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task ResetPassword_WhenValidFields_SetsSuccessState()
    {
        // Arrange
        var viewModel = new ForgotPasswordViewModel(this.recoveryManager.Object);
        this.recoveryManager
            .Setup(recoveryManagerMock => recoveryManagerMock.IsPasswordValid(It.IsAny<string>()))
            .Returns(true);
        this.recoveryManager
            .Setup(recoveryManagerMock => recoveryManagerMock.ResetPasswordAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(ForgotPasswordState.PasswordResetSuccess);

        // Act
        await viewModel.ResetPassword("StrongP@ss1", "123456");

        // Assert
        viewModel.State.Value.Should().Be(ForgotPasswordState.PasswordResetSuccess);
        viewModel.ValidationError.Should().BeEmpty();
    }

    /// <summary>
    /// When an empty token is supplied for verification, state transitions to
    /// <see cref="ForgotPasswordState.Error"/> and a validation message is set.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task VerifyToken_WhenEmpty_SetsErrorState()
    {
        // Arrange
        var viewModel = new ForgotPasswordViewModel(this.recoveryManager.Object);

        // Act
        await viewModel.VerifyToken(string.Empty);

        // Assert
        viewModel.State.Value.Should().Be(ForgotPasswordState.Error);
        viewModel.ValidationError.Should().NotBeEmpty();
    }

    /// <summary>
    /// When a non-empty token is supplied, the recovery manager validates it and state transitions to
    /// <see cref="ForgotPasswordState.TokenValid"/>.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task VerifyToken_WhenValid_SetsVerifiedState()
    {
        // Arrange
        var viewModel = new ForgotPasswordViewModel(this.recoveryManager.Object);
        this.recoveryManager
            .Setup(recoveryManagerMock => recoveryManagerMock.VerifyTokenAsync(It.IsAny<string>()))
            .ReturnsAsync(ForgotPasswordState.TokenValid);

        // Act
        await viewModel.VerifyToken("123456");

        // Assert
        viewModel.State.Value.Should().Be(ForgotPasswordState.TokenValid);
        viewModel.ValidationError.Should().BeEmpty();
    }
}
