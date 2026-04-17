// <copyright file="TwoFactorViewModelTests.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

using BankApp.Application.DTOs.Auth;
using BankApp.Desktop.Enums;
using BankApp.Desktop.Utilities;
using BankApp.Desktop.ViewModels;
using ErrorOr;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace BankApp.Desktop.Tests.ViewModels;

/// <summary>
/// Tests for <see cref="TwoFactorViewModel"/>.
/// </summary>
public class TwoFactorViewModelTests
{
    private readonly Mock<IApiClient> apiClient = new Mock<IApiClient>();
    private readonly Mock<ICountdownTimer> countdownTimer = new Mock<ICountdownTimer>();

    /// <summary>
    /// When the OTP code is shorter than six digits, the view model stays in Idle and reports an error
    /// without making any API call.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task VerifyOtp_WhenCodeTooShort_SetsErrorAndDoesNotCallApi()
    {
        // Arrange
        var viewModel = new TwoFactorViewModel(this.apiClient.Object, this.countdownTimer.Object, NullLogger<TwoFactorViewModel>.Instance);

        // Act
        viewModel.OtpCode = "123";
        await viewModel.VerifyOtp();

        // Assert
        viewModel.State.Value.Should().Be(TwoFactorState.Idle);
        viewModel.HasError.Should().BeTrue();
        this.apiClient.Verify(
            client => client.PostAsync<VerifyOTPRequest, LoginSuccessResponse>(It.IsAny<string>(), It.IsAny<VerifyOTPRequest>()),
            Times.Never);
    }

    /// <summary>
    /// When <see cref="IApiClient.CurrentUserId"/> is null the session has expired;
    /// the view model transitions to <see cref="TwoFactorState.InvalidOTP"/>.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task VerifyOtp_WhenUserIdIsNull_SetsInvalidOtpState()
    {
        // Arrange
        var viewModel = new TwoFactorViewModel(this.apiClient.Object, this.countdownTimer.Object, NullLogger<TwoFactorViewModel>.Instance);
        this.apiClient.Setup(client => client.CurrentUserId).Returns((int?)null);

        // Act
        viewModel.OtpCode = "123456";
        await viewModel.VerifyOtp();

        // Assert
        viewModel.State.Value.Should().Be(TwoFactorState.InvalidOTP);
        viewModel.HasError.Should().BeTrue();
    }

    /// <summary>
    /// When the API accepts the OTP, the view model transitions to <see cref="TwoFactorState.Success"/>.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task VerifyOtp_WhenApiSucceeds_SetsSuccessState()
    {
        // Arrange
        var viewModel = new TwoFactorViewModel(this.apiClient.Object, this.countdownTimer.Object, NullLogger<TwoFactorViewModel>.Instance);
        this.apiClient.Setup(client => client.CurrentUserId).Returns(1);

        var successResponse = new LoginSuccessResponse { Token = "token", UserId = 1 };
        this.apiClient
            .Setup(client => client.PostAsync<VerifyOTPRequest, LoginSuccessResponse>(It.IsAny<string>(), It.IsAny<VerifyOTPRequest>()))
            .ReturnsAsync(successResponse);
        this.apiClient.Setup(client => client.SetToken(It.IsAny<string>()));

        // Act
        viewModel.OtpCode = "123456";
        await viewModel.VerifyOtp();

        // Assert
        viewModel.State.Value.Should().Be(TwoFactorState.Success);
        viewModel.HasError.Should().BeFalse();
    }

    /// <summary>
    /// When the API rejects the OTP, the view model transitions to <see cref="TwoFactorState.InvalidOTP"/>.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task VerifyOtp_WhenApiFails_SetsInvalidOtpState()
    {
        // Arrange
        var viewModel = new TwoFactorViewModel(this.apiClient.Object, this.countdownTimer.Object, NullLogger<TwoFactorViewModel>.Instance);
        this.apiClient.Setup(client => client.CurrentUserId).Returns(1);

        this.apiClient
            .Setup(client => client.PostAsync<VerifyOTPRequest, LoginSuccessResponse>(It.IsAny<string>(), It.IsAny<VerifyOTPRequest>()))
            .ReturnsAsync(Error.Validation("invalid_otp"));

        // Act
        viewModel.OtpCode = "123456";
        await viewModel.VerifyOtp();

        // Assert
        viewModel.State.Value.Should().Be(TwoFactorState.InvalidOTP);
        viewModel.HasError.Should().BeTrue();
    }

    /// <summary>
    /// When resend is called a second time before the cooldown expires, no additional API call is made.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task ResendOtp_WhenInCooldown_DoesNotMakeSecondApiCall()
    {
        // Arrange
        var viewModel = new TwoFactorViewModel(this.apiClient.Object, this.countdownTimer.Object, NullLogger<TwoFactorViewModel>.Instance);
        this.apiClient.Setup(client => client.CurrentUserId).Returns(1);
        this.apiClient
            .Setup(client => client.PostAsync<object?, object>(It.IsAny<string>(), It.IsAny<object?>()))
            .ReturnsAsync(new object());

        await viewModel.ResendOtp(); // first call succeeds and starts cooldown

        // Act
        await viewModel.ResendOtp(); // second call is throttled

        // Assert
        this.apiClient.Verify(
            client => client.PostAsync<object?, object>(It.IsAny<string>(), It.IsAny<object?>()),
            Times.Once);
    }

    /// <summary>
    /// When resend is allowed, the API is called and the countdown timer is started.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task ResendOtp_WhenCanResend_CallsApiAndStartsTimer()
    {
        // Arrange
        var viewModel = new TwoFactorViewModel(this.apiClient.Object, this.countdownTimer.Object, NullLogger<TwoFactorViewModel>.Instance);
        this.apiClient.Setup(client => client.CurrentUserId).Returns(1);
        this.apiClient
            .Setup(client => client.PostAsync<object?, object>(It.IsAny<string>(), It.IsAny<object?>()))
            .ReturnsAsync(new object());

        // Act
        await viewModel.ResendOtp();

        // Assert
        this.countdownTimer.Verify(timer => timer.Start(), Times.Once);
        viewModel.SecondsRemaining.Should().Be(30);
    }
}
