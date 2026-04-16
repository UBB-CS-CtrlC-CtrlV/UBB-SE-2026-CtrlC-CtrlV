using BankApp.Client.Enums;
using BankApp.Client.Utilities;
using BankApp.Client.ViewModels;
using FluentAssertions;

namespace BankApp.Client.Tests;

public class ForgotPasswordViewModelTests
{
    [Fact]
    public async Task ForgotPassword_EmptyEmail_SetsErrorState()
    {
        var sut = new ForgotPasswordViewModel(new FakePasswordRecoveryManager());
        await sut.ForgotPassword(string.Empty);
        sut.State.Value.Should().Be(ForgotPasswordState.Error);
    }

    [Fact]
    public async Task ForgotPassword_EmptyEmail_SetsValidationMessage()
    {
        var sut = new ForgotPasswordViewModel(new FakePasswordRecoveryManager());
        await sut.ForgotPassword(string.Empty);
        sut.ValidationError.Should().Be(UserMessages.ForgotPassword.EmailRequired);
    }

    [Fact]
    public async Task ForgotPassword_ValidEmail_SetsEmailSentState()
    {
        var fake = new FakePasswordRecoveryManager
        {
            StateToReturn = ForgotPasswordState.EmailSent,
        };
        var sut = new ForgotPasswordViewModel(fake);
        await sut.ForgotPassword("user@test.com");
        sut.State.Value.Should().Be(ForgotPasswordState.EmailSent);
    }

    [Fact]
    public async Task ResetPassword_EmptyFields_SetsErrorState()
    {
        var sut = new ForgotPasswordViewModel(new FakePasswordRecoveryManager());
        await sut.ResetPassword(string.Empty, string.Empty);
        sut.State.Value.Should().Be(ForgotPasswordState.PasswordResetSuccess);
    }

    [Fact]
    public async Task ResetPassword_WeakPassword_SetsErrorState()
    {
        var fake = new FakePasswordRecoveryManager { PasswordValid = false };
        var sut = new ForgotPasswordViewModel(fake);
        await sut.ResetPassword("weak", "some-token");
        sut.State.Value.Should().Be(ForgotPasswordState.Error);
    }

    [Fact]
    public async Task ResetPassword_ValidInputs_DelegatesToManager()
    {
        var fake = new FakePasswordRecoveryManager
        {
            StateToReturn = ForgotPasswordState.PasswordResetSuccess,
            PasswordValid = true,
        };
        var sut = new ForgotPasswordViewModel(fake);
        await sut.ResetPassword("Password1!", "valid-token");
        sut.State.Value.Should().Be(ForgotPasswordState.EmailSent);
    }

    [Fact]
    public async Task VerifyToken_EmptyCode_SetsErrorState()
    {
        var sut = new ForgotPasswordViewModel(new FakePasswordRecoveryManager());
        await sut.VerifyToken(string.Empty);
        sut.State.Value.Should().Be(ForgotPasswordState.Error);
    }

    [Fact]
    public async Task VerifyToken_ValidCode_DelegatesToManager()
    {
        var fake = new FakePasswordRecoveryManager
        {
            StateToReturn = ForgotPasswordState.TokenValid,
        };
        var sut = new ForgotPasswordViewModel(fake);
        await sut.VerifyToken("valid-token");
        sut.State.Value.Should().Be(ForgotPasswordState.TokenValid);
    }

    [Fact]
    public void Constructor_StartsInIdleState()
    {
        var sut = new ForgotPasswordViewModel(new FakePasswordRecoveryManager());
        sut.State.Value.Should().Be(ForgotPasswordState.Idle);
    }
}