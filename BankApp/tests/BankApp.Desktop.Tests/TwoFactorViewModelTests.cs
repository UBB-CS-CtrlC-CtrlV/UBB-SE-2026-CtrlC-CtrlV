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

public class TwoFactorViewModelTests
{
    private readonly ApiClient apiClient;
    private readonly ICountdownTimer countdownTimer;

    public TwoFactorViewModelTests()
    {
        var configBuilder = new Microsoft.Extensions.Configuration.ConfigurationBuilder();
        configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
        {
            { "ApiBaseUrl", "http://localhost" }
        });
        this.apiClient = Substitute.For<ApiClient>(configBuilder.Build(), NullLogger<ApiClient>.Instance);
        this.countdownTimer = Substitute.For<ICountdownTimer>();
    }

    [Fact]
    public async Task VerifyOtp_WhenEmptyOrTooShort_SetsErrorAndDoesNotCallApi()
    {
        // Arrange
        var vm = new TwoFactorViewModel(this.apiClient, this.countdownTimer, NullLogger<TwoFactorViewModel>.Instance);

        // Act
        vm.OtpCode = "123";
        await vm.VerifyOtp();

        // Assert
        vm.State.Value.Should().Be(TwoFactorState.Idle);
        vm.HasError.Should().BeTrue();
    }

    [Fact]
    public async Task VerifyOtp_WhenUserIdIsNull_SetsInvalidOtpState()
    {
        // Arrange
        var vm = new TwoFactorViewModel(this.apiClient, this.countdownTimer, NullLogger<TwoFactorViewModel>.Instance);
        this.apiClient.CurrentUserId = null;

        // Act
        vm.OtpCode = "123456";
        await vm.VerifyOtp();

        // Assert
        vm.State.Value.Should().Be(TwoFactorState.InvalidOTP);
        vm.HasError.Should().BeTrue();
    }

    [Fact]
    public async Task VerifyOtp_WhenApiSucceeds_SetsSuccessState()
    {
        // Arrange
        var vm = new TwoFactorViewModel(this.apiClient, this.countdownTimer, NullLogger<TwoFactorViewModel>.Instance);
        this.apiClient.CurrentUserId = 1;

        var successResponse = new LoginSuccessResponse { Token = "token", UserId = 1 };
        this.apiClient.PostAsync<VerifyOTPRequest, LoginSuccessResponse>(Arg.Any<string>(), Arg.Any<VerifyOTPRequest>())
            .Returns(Task.FromResult<ErrorOr<LoginSuccessResponse>>(successResponse));

        // Act
        vm.OtpCode = "123456";
        await vm.VerifyOtp();

        // Assert
        vm.State.Value.Should().Be(TwoFactorState.Success);
        vm.HasError.Should().BeFalse();
    }

    [Fact]
    public async Task VerifyOtp_WhenApiFails_SetsInvalidOtpState()
    {
        // Arrange
        var vm = new TwoFactorViewModel(this.apiClient, this.countdownTimer, NullLogger<TwoFactorViewModel>.Instance);
        this.apiClient.CurrentUserId = 1;

        this.apiClient.PostAsync<VerifyOTPRequest, LoginSuccessResponse>(Arg.Any<string>(), Arg.Any<VerifyOTPRequest>())
            .Returns(Task.FromResult<ErrorOr<LoginSuccessResponse>>(Error.Validation("invalid_otp")));

        // Act
        vm.OtpCode = "123456";
        await vm.VerifyOtp();

        // Assert
        vm.State.Value.Should().Be(TwoFactorState.InvalidOTP);
        vm.HasError.Should().BeTrue();
    }

    [Fact]
    public async Task ResendOtp_WhenCannotResend_DoesNothing()
    {
        // Arrange
        var vm = new TwoFactorViewModel(this.apiClient, this.countdownTimer, NullLogger<TwoFactorViewModel>.Instance);

        // Push it into a cooldown state
        await vm.ResendOtp();
        this.apiClient.ClearReceivedCalls();

        // Act
        await vm.ResendOtp();

        // Assert
        await this.apiClient.DidNotReceiveWithAnyArgs().PostAsync<object?, object>(Arg.Any<string>(), Arg.Any<object>());
    }

    [Fact]
    public async Task ResendOtp_WhenCanResend_CallsApiAndStartsTimer()
    {
        // Arrange
        var vm = new TwoFactorViewModel(this.apiClient, this.countdownTimer, NullLogger<TwoFactorViewModel>.Instance);
        this.apiClient.CurrentUserId = 1;

        this.apiClient.PostAsync<object?, object>(Arg.Any<string>(), Arg.Any<object>())
            .Returns(Task.FromResult<ErrorOr<object>>(new object()));

        // Act
        await vm.ResendOtp();

        // Assert
        this.countdownTimer.Received().Start();
        vm.SecondsRemaining.Should().Be(30);
    }
}
