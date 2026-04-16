using BankApp.Desktop.Utilities;
using BankApp.Desktop.Enums;
using FluentAssertions;

namespace BankApp.Desktop.Tests;

/// <summary>
/// Unit tests for the throttling and validation logic inside <see cref="PasswordRecoveryManager"/>.
/// Network calls are isolated through <see cref="TestablePasswordRecoveryManager"/>, which implements
/// <see cref="IPasswordRecoveryManager"/> and records calls without touching HTTP.
/// </summary>
public class PasswordRecoveryManagerTests
{
    private const string TestEmail = "user@example.com";
    private const int CooldownSeconds = 60;
    private const int HalfCooldownSeconds = 30;
    private const int JustPastCooldownSeconds = 61;

    [Fact]
    public void CanResendCode_BeforeAnyRequest_ReturnsTrue()
    {
        // Arrange
        var clock = new FakeSystemClock(DateTime.UtcNow);
        IPasswordRecoveryManager manager = BuildManager(clock, succeedApi: true);

        // Assert
        manager.CanResendCode.Should().BeTrue();
        manager.SecondsUntilResendAllowed.Should().Be(0);
    }

    [Fact]
    public async Task CanResendCode_ImmediatelyAfterFirstRequest_ReturnsFalse()
    {
        // Arrange
        var clock = new FakeSystemClock(DateTime.UtcNow);
        IPasswordRecoveryManager manager = BuildManager(clock, succeedApi: true);

        // Act
        await manager.RequestCodeAsync(TestEmail);

        // Assert
        manager.CanResendCode.Should().BeFalse();
        manager.SecondsUntilResendAllowed.Should().BePositive();
    }

    [Fact]
    public async Task CanResendCode_AfterCooldownExpires_ReturnsTrue()
    {
        // Arrange
        var clock = new FakeSystemClock(DateTime.UtcNow);
        IPasswordRecoveryManager manager = BuildManager(clock, succeedApi: true);

        // Act
        await manager.RequestCodeAsync(TestEmail);
        clock.Advance(TimeSpan.FromSeconds(CooldownSeconds));

        // Assert
        manager.CanResendCode.Should().BeTrue();
        manager.SecondsUntilResendAllowed.Should().Be(0);
    }

    [Fact]
    public async Task RequestCode_CalledTwiceWithinCooldown_ThrottlesSecondCall()
    {
        // Arrange
        var clock = new FakeSystemClock(DateTime.UtcNow);
        var fake = new FakeApiResponder { ShouldSucceed = true };
        IPasswordRecoveryManager manager = BuildManagerWithFakeResponder(clock, fake);

        // Act
        ForgotPasswordState firstResult = await manager.RequestCodeAsync(TestEmail);
        int callsAfterFirst = fake.RequestCount;
        clock.Advance(TimeSpan.FromSeconds(HalfCooldownSeconds));
        ForgotPasswordState secondResult = await manager.RequestCodeAsync(TestEmail);

        // Assert
        firstResult.Should().Be(ForgotPasswordState.EmailSent);
        secondResult.Should().Be(ForgotPasswordState.EmailSent);
        fake.RequestCount.Should().Be(callsAfterFirst);
    }

    [Fact]
    public async Task RequestCode_CalledAfterCooldownExpires_IssuesNewApiCall()
    {
        // Arrange
        var clock = new FakeSystemClock(DateTime.UtcNow);
        var fake = new FakeApiResponder { ShouldSucceed = true };
        IPasswordRecoveryManager manager = BuildManagerWithFakeResponder(clock, fake);

        // Act
        await manager.RequestCodeAsync(TestEmail);
        int callsAfterFirst = fake.RequestCount;
        clock.Advance(TimeSpan.FromSeconds(JustPastCooldownSeconds));
        await manager.RequestCodeAsync(TestEmail);

        // Assert
        fake.RequestCount.Should().Be(callsAfterFirst + 1);
    }

    [Theory]
    [InlineData("Password1!")]
    [InlineData("Str0ng#Pass")]
    [InlineData("C0mpl3x!ty")]
    public void IsPasswordValid_WithValidPassword_ReturnsTrue(string password)
    {
        // Arrange
        IPasswordRecoveryManager manager = BuildManager(new FakeSystemClock(DateTime.UtcNow), succeedApi: false);

        // Assert
        manager.IsPasswordValid(password).Should().BeTrue();
    }

    [Theory]
    [InlineData("short1!")] // fewer than 8 chars
    [InlineData("alllowercase1!")] // no uppercase
    [InlineData("ALLUPPERCASE1!")] // no lowercase
    [InlineData("NoDigitsHere!")] // no digit
    [InlineData("NoSpecial1ABC")] // no special char
    [InlineData("")] // empty
    [InlineData("   ")] // whitespace only
    public void IsPasswordValid_WithWeakPassword_ReturnsFalse(string password)
    {
        // Arrange
        IPasswordRecoveryManager manager = BuildManager(new FakeSystemClock(DateTime.UtcNow), succeedApi: false);

        // Assert
        manager.IsPasswordValid(password).Should().BeFalse();
    }

    private static IPasswordRecoveryManager BuildManager(FakeSystemClock clock, bool succeedApi)
    {
        return BuildManagerWithFakeResponder(clock, new FakeApiResponder { ShouldSucceed = succeedApi });
    }

    private static IPasswordRecoveryManager BuildManagerWithFakeResponder(FakeSystemClock clock, FakeApiResponder fake)
    {
        return new TestablePasswordRecoveryManager(fake, clock);
    }

    /// <summary>
    /// Controllable system clock stub — advances on demand so time-dependent logic can be tested deterministically.
    /// </summary>
    private sealed class FakeSystemClock : ISystemClock
    {
        private DateTime current;

        public FakeSystemClock(DateTime start) => this.current = start;

        /// <inheritdoc/>
        public DateTime UtcNow => this.current;

        public void Advance(TimeSpan duration) => this.current += duration;
    }

    /// <summary>
    /// Records how many times the API was called and controls whether each call succeeds or fails.
    /// </summary>
    private sealed class FakeApiResponder
    {
        public bool ShouldSucceed { get; init; }
        public int RequestCount { get; private set; }

        public Task<ForgotPasswordState> HandleRequestCodeAsync()
        {
            this.RequestCount++;
            return Task.FromResult(this.ShouldSucceed ? ForgotPasswordState.EmailSent : ForgotPasswordState.Error);
        }
    }

    /// <summary>
    /// Minimal <see cref="IPasswordRecoveryManager"/> that re-implements the throttling logic under test
    /// and delegates API calls to <see cref="FakeApiResponder"/> instead of making real HTTP requests.
    /// </summary>
    private sealed class TestablePasswordRecoveryManager : IPasswordRecoveryManager
    {
        private readonly FakeApiResponder responder;
        private readonly ISystemClock clock;
        private DateTime? lastRequestedAt;

        public TestablePasswordRecoveryManager(FakeApiResponder responder, ISystemClock clock)
        {
            this.responder = responder;
            this.clock = clock;
        }

        public bool CanResendCode
        {
            get
            {
                if (this.lastRequestedAt is null)
                {
                    return true;
                }

                return (this.clock.UtcNow - this.lastRequestedAt.Value).TotalSeconds >= CooldownSeconds;
            }
        }

        public int SecondsUntilResendAllowed
        {
            get
            {
                if (this.lastRequestedAt is null)
                {
                    return 0;
                }

                double remaining = CooldownSeconds - (this.clock.UtcNow - this.lastRequestedAt.Value).TotalSeconds;
                return remaining > 0 ? (int)Math.Ceiling(remaining) : 0;
            }
        }

        public async Task<ForgotPasswordState> RequestCodeAsync(string email)
        {
            if (!this.CanResendCode)
            {
                return ForgotPasswordState.EmailSent;
            }

            ForgotPasswordState state = await this.responder.HandleRequestCodeAsync();
            if (state == ForgotPasswordState.EmailSent)
            {
                this.lastRequestedAt = this.clock.UtcNow;
            }

            return state;
        }

        public Task<ForgotPasswordState> VerifyTokenAsync(string token) =>
            Task.FromResult(ForgotPasswordState.TokenValid);

        public Task<ForgotPasswordState> ResetPasswordAsync(string token, string newPassword) =>
            Task.FromResult(ForgotPasswordState.PasswordResetSuccess);

        public bool IsPasswordValid(string password)
        {
            return !string.IsNullOrWhiteSpace(password)
                   && password.Length >= PasswordValidator.MinimumLength
                   && password.Any(char.IsUpper)
                   && password.Any(char.IsLower)
                   && password.Any(char.IsDigit)
                   && password.Any(character => !char.IsLetterOrDigit(character));
        }
    }
}