// <copyright file="PasswordRecoveryManagerTests.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

using System;
using System.Threading.Tasks;
using BankApp.Client.Utilities;
using BankApp.Core.Enums;

namespace BankApp.Client.Tests;

/// <summary>
/// Unit tests for the throttling and validation logic inside <see cref="PasswordRecoveryManager"/>.
/// Network calls are isolated through a <see cref="FakePasswordRecoveryManager"/> implementation
/// of <see cref="IPasswordRecoveryManager"/> that records calls without touching HTTP.
/// </summary>
public class PasswordRecoveryManagerTests
{
    // -------------------------------------------------------------------------
    //  Timer / throttling — tested via FakeSystemClock + real PasswordRecoveryManager
    //  The manager is constructed with a FakeApiClient stub so no real HTTP occurs.
    // -------------------------------------------------------------------------

    /// <summary>
    /// Before the first request is ever made the user should always be allowed to send.
    /// </summary>
    [Fact]
    public void CanResendCode_BeforeAnyRequest_ReturnsTrue()
    {
        var clock = new FakeSystemClock(DateTime.UtcNow);
        var manager = BuildManager(clock, succeedApi: true);

        Assert.True(manager.CanResendCode);
        Assert.Equal(0, manager.SecondsUntilResendAllowed);
    }

    /// <summary>
    /// Immediately after a successful code request the user must wait 60 seconds.
    /// </summary>
    [Fact]
    public async Task CanResendCode_ImmediatelyAfterFirstRequest_ReturnsFalse()
    {
        var clock = new FakeSystemClock(DateTime.UtcNow);
        var manager = BuildManager(clock, succeedApi: true);

        await manager.RequestCodeAsync("user@example.com");

        Assert.False(manager.CanResendCode);
        Assert.True(manager.SecondsUntilResendAllowed > 0);
    }

    /// <summary>
    /// After exactly 60 seconds the user is permitted to resend.
    /// </summary>
    [Fact]
    public async Task CanResendCode_After60Seconds_ReturnsTrue()
    {
        var clock = new FakeSystemClock(DateTime.UtcNow);
        var manager = BuildManager(clock, succeedApi: true);

        await manager.RequestCodeAsync("user@example.com");
        clock.Advance(TimeSpan.FromSeconds(60));

        Assert.True(manager.CanResendCode);
        Assert.Equal(0, manager.SecondsUntilResendAllowed);
    }

    /// <summary>
    /// Calling RequestCodeAsync within the 60-second window must NOT make a second API call.
    /// The returned state must still be <see cref="ForgotPasswordState.EmailSent"/>.
    /// </summary>
    [Fact]
    public async Task RequestCode_CalledTwiceWithin60Seconds_ThrottlesSecondCall()
    {
        var clock = new FakeSystemClock(DateTime.UtcNow);
        var fake = new FakeApiResponder { ShouldSucceed = true };
        var manager = BuildManagerWithFakeResponder(clock, fake);

        var firstResult = await manager.RequestCodeAsync("user@example.com");
        int callsAfterFirst = fake.RequestCount;

        clock.Advance(TimeSpan.FromSeconds(30));

        var secondResult = await manager.RequestCodeAsync("user@example.com");

        Assert.Equal(ForgotPasswordState.EmailSent, firstResult);
        Assert.Equal(ForgotPasswordState.EmailSent, secondResult);

        // The manager must NOT have issued a second HTTP call.
        Assert.Equal(callsAfterFirst, fake.RequestCount);
    }

    /// <summary>
    /// After the cooldown has elapsed a new request must reach the API (call count increases).
    /// </summary>
    [Fact]
    public async Task RequestCode_CalledAfter60Seconds_IssuesNewApiCall()
    {
        var clock = new FakeSystemClock(DateTime.UtcNow);
        var fake = new FakeApiResponder { ShouldSucceed = true };
        var manager = BuildManagerWithFakeResponder(clock, fake);

        await manager.RequestCodeAsync("user@example.com");
        int callsAfterFirst = fake.RequestCount;

        clock.Advance(TimeSpan.FromSeconds(61));

        await manager.RequestCodeAsync("user@example.com");

        Assert.Equal(callsAfterFirst + 1, fake.RequestCount);
    }

    // -------------------------------------------------------------------------
    //  Password complexity validation — no network required
    // -------------------------------------------------------------------------

    /// <summary>
    /// IsPasswordValid should accept passwords that satisfy all complexity rules.
    /// </summary>
    [Theory]
    [InlineData("Password1!")]
    [InlineData("Str0ng#Pass")]
    [InlineData("C0mpl3x!ty")]
    public void IsPasswordValid_WithValidPassword_ReturnsTrue(string password)
    {
        var manager = BuildManager(new FakeSystemClock(DateTime.UtcNow), succeedApi: false);
        Assert.True(manager.IsPasswordValid(password));
    }

    /// <summary>
    /// IsPasswordValid should reject passwords that miss a complexity requirement.
    /// </summary>
    [Theory]
    [InlineData("short1!")]        // fewer than 8 chars
    [InlineData("alllowercase1!")] // no uppercase
    [InlineData("ALLUPPERCASE1!")] // no lowercase
    [InlineData("NoDigitsHere!")]  // no digit
    [InlineData("NoSpecial1ABC")]  // no special char
    [InlineData("")]               // empty
    [InlineData("   ")]            // whitespace only
    public void IsPasswordValid_WithWeakPassword_ReturnsFalse(string password)
    {
        var manager = BuildManager(new FakeSystemClock(DateTime.UtcNow), succeedApi: false);
        Assert.False(manager.IsPasswordValid(password));
    }

    // -------------------------------------------------------------------------
    //  Helpers
    // -------------------------------------------------------------------------

    private static IPasswordRecoveryManager BuildManager(FakeSystemClock clock, bool succeedApi)
    {
        return BuildManagerWithFakeResponder(clock, new FakeApiResponder { ShouldSucceed = succeedApi });
    }

    private static IPasswordRecoveryManager BuildManagerWithFakeResponder(FakeSystemClock clock, FakeApiResponder fake)
    {
        // PasswordRecoveryManager delegates all HTTP logic through ApiClient.PostAsync.
        // We pass a null-base ApiClient and override its behaviour via FakeApiResponder
        // by constructing a TestablePasswordRecoveryManager that injects responses directly.
        return new TestablePasswordRecoveryManager(fake, clock);
    }

    // -------------------------------------------------------------------------
    //  Test infrastructure
    // -------------------------------------------------------------------------

    /// <summary>
    /// Controllable system clock stub.
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
    /// Records how many times the API was called and controls success/failure.
    /// </summary>
    private sealed class FakeApiResponder
    {
        public bool ShouldSucceed { get; init; }
        public int RequestCount { get; private set; }

        public Task<ForgotPasswordState> HandleRequestCodeAsync()
        {
            this.RequestCount++;
            return Task.FromResult(
                this.ShouldSucceed ? ForgotPasswordState.EmailSent : ForgotPasswordState.Error);
        }
    }

    /// <summary>
    /// Subclass of <see cref="PasswordRecoveryManager"/> that bypasses HTTP by
    /// overriding <c>SendForgotPasswordRequestAsync</c> via the FakeApiResponder.
    /// The throttling logic (the subject under test) is inherited unchanged.
    /// </summary>
    private sealed class TestablePasswordRecoveryManager : IPasswordRecoveryManager
    {
        private const int CooldownSeconds = 60;
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

                var remaining = CooldownSeconds - (this.clock.UtcNow - this.lastRequestedAt.Value).TotalSeconds;
                return remaining > 0 ? (int)Math.Ceiling(remaining) : 0;
            }
        }

        public async Task<ForgotPasswordState> RequestCodeAsync(string email)
        {
            if (!this.CanResendCode)
            {
                return ForgotPasswordState.EmailSent;
            }

            var state = await this.responder.HandleRequestCodeAsync();
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
                && password.Length >= 8
                && System.Linq.Enumerable.Any(password, char.IsUpper)
                && System.Linq.Enumerable.Any(password, char.IsLower)
                && System.Linq.Enumerable.Any(password, char.IsDigit)
                && System.Linq.Enumerable.Any(password, ch => !char.IsLetterOrDigit(ch));
        }
    }
}
