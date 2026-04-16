using BankApp.Client.Enums;
using BankApp.Client.Utilities;
using BankApp.Client.ViewModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;

namespace BankApp.Client.Tests;

public class TwoFactorViewModelTests
{
    private static (TwoFactorViewModel sut, FakeCountdownTimer timer) CreateViewModel()
    {
        IConfiguration config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ApiBaseUrl"] = "http://localhost",
            })
            .Build();

        var timer = new FakeCountdownTimer();
        var sut = new TwoFactorViewModel(
            new ApiClient(config, NullLogger<ApiClient>.Instance),
            timer,
            NullLogger<TwoFactorViewModel>.Instance);

        return (sut, timer);
    }

    [Fact]
    public async Task VerifyOtp_EmptyCode_SetsHasError()
    {
        var (sut, _) = CreateViewModel();
        await sut.VerifyOtp();
        Assert.True(sut.HasError);
    }

    [Fact]
    public async Task VerifyOtp_ShortCode_SetsHasError()
    {
        var (sut, _) = CreateViewModel();
        sut.OtpCode = "123";
        await sut.VerifyOtp();
        Assert.True(sut.HasError);
    }

    [Fact]
    public async Task VerifyOtp_ShortCode_SetsCorrectErrorMessage()
    {
        var (sut, _) = CreateViewModel();
        sut.OtpCode = "123";
        await sut.VerifyOtp();
        Assert.Equal(UserMessages.TwoFactor.InvalidCodeFormat, sut.ErrorMessage);
    }

    [Fact]
    public async Task ResendOtp_WhenCannotResend_DoesNotStartTimer()
    {
        var (sut, timer) = CreateViewModel();
        sut.OtpCode = "123456";

        // Start a countdown so CanResend is false
        await sut.ResendOtp();
        timer.Stop();
        bool wasRunning = timer.IsRunning;

        await sut.ResendOtp();
        Assert.False(timer.IsRunning);
    }

    [Fact]
    public async Task ResendOtp_WhenCanResend_StartsTimer()
    {
        var (sut, timer) = CreateViewModel();
        await sut.ResendOtp();
        Assert.True(timer.IsRunning);
    }

    [Fact]
    public void CountdownTimer_Tick_DecrementsSecondsRemaining()
    {
        var (sut, timer) = CreateViewModel();
        sut.SecondsRemaining = 10; // set via internal access (InternalsVisibleTo)
        timer.FireTick();
        Assert.Equal(9, sut.SecondsRemaining);
    }

    [Fact]
    public void Constructor_StartsInIdleState()
    {
        var (sut, _) = CreateViewModel();
        Assert.Equal(TwoFactorState.Idle, sut.State.Value);
    }
}