using BankApp.Client.Enums;
using BankApp.Client.Utilities;
using BankApp.Client.ViewModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;

namespace BankApp.Client.Tests;

public class TwoFactorViewModelTests
{
    private static TwoFactorViewModel CreateViewModel(Mock<ICountdownTimer> timerMock)
    {
        IConfiguration config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ApiBaseUrl"] = "http://localhost",
            })
            .Build();

        return new TwoFactorViewModel(
            new ApiClient(config, NullLogger<ApiClient>.Instance),
            timerMock.Object,
            NullLogger<TwoFactorViewModel>.Instance);
    }

    [Fact]
    public async Task VerifyOtp_EmptyCode_SetsHasError()
    {
        var sut = CreateViewModel(new Mock<ICountdownTimer>());
        await sut.VerifyOtp();
        sut.HasError.Should().BeTrue();
    }

    [Fact]
    public async Task VerifyOtp_ShortCode_SetsHasError()
    {
        var sut = CreateViewModel(new Mock<ICountdownTimer>());
        sut.OtpCode = "123";
        await sut.VerifyOtp();
        sut.HasError.Should().BeTrue();
    }

    [Fact]
    public async Task VerifyOtp_ShortCode_SetsCorrectErrorMessage()
    {
        var sut = CreateViewModel(new Mock<ICountdownTimer>());
        sut.OtpCode = "123";
        await sut.VerifyOtp();
        sut.ErrorMessage.Should().Be(UserMessages.TwoFactor.InvalidCodeFormat);
    }

    [Fact]
    public async Task ResendOtp_WhenCanResend_StartsTimer()
    {
        var timerMock = new Mock<ICountdownTimer>();
        timerMock.Setup(t => t.Start());

        var sut = CreateViewModel(timerMock);
        await sut.ResendOtp();

        timerMock.Verify(t => t.Start(), Times.Once);
    }

    [Fact]
    public async Task ResendOtp_WhenCannotResend_DoesNotStartTimer()
    {
        var timerMock = new Mock<ICountdownTimer>();
        var sut = CreateViewModel(timerMock);

        // first call starts the countdown so CanResend becomes false
        await sut.ResendOtp();
        timerMock.Invocations.Clear();

        // SecondsRemaining > 0 means CanResend is false
        sut.SecondsRemaining = 30;
        await sut.ResendOtp();

        timerMock.Verify(t => t.Start(), Times.Never);
    }

    [Fact]
    public void Constructor_StartsInIdleState()
    {
        var sut = CreateViewModel(new Mock<ICountdownTimer>());
        sut.State.Value.Should().Be(TwoFactorState.Idle);
    }
}