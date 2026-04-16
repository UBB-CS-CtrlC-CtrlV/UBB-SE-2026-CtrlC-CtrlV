using BankApp.Client.Enums;
using BankApp.Client.Utilities;
using BankApp.Client.ViewModels;

namespace BankApp.Client.Tests;

public class ForgotPasswordViewModelTests
{
    [Fact]
    public async Task ForgotPassword_EmptyEmail_SetsErrorState()
    {
        var sut = new ForgotPasswordViewModel(new FakePasswordRecoveryManager());
        await sut.ForgotPassword(string.Empty);
        Assert.Equal(ForgotPasswordState.Error, sut.State.Value);
    }

    [Fact]
    public async Task ForgotPassword_EmptyEmail_SetsValidationMessage()
    {
        var sut = new ForgotPasswordViewModel(new FakePasswordRecoveryManager());
        await sut.ForgotPassword(string.Empty);
        Assert.Equal(UserMessages.ForgotPassword.EmailRequired, sut.ValidationError);
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
        Assert.Equal(ForgotPasswordState.EmailSent, sut.State.Value);
    }

    [Fact]
    public async Task ResetPassword_EmptyFields_SetsErrorState()
    {
        var sut = new ForgotPasswordViewModel(new FakePasswordRecoveryManager());
        await sut.ResetPassword(string.Empty, string.Empty);
        Assert.Equal(ForgotPasswordState.Error, sut.State.Value);
    }

    [Fact]
    public async Task ResetPassword_WeakPassword_SetsErrorState()
    {
        var fake = new FakePasswordRecoveryManager { PasswordValid = false };
        var sut = new ForgotPasswordViewModel(fake);
        await sut.ResetPassword("weak", "some-token");
        Assert.Equal(ForgotPasswordState.Error, sut.State.Value);
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
        Assert.Equal(ForgotPasswordState.PasswordResetSuccess, sut.State.Value);
    }

    [Fact]
    public async Task VerifyToken_EmptyCode_SetsErrorState()
    {
        var sut = new ForgotPasswordViewModel(new FakePasswordRecoveryManager());
        await sut.VerifyToken(string.Empty);
        Assert.Equal(ForgotPasswordState.Error, sut.State.Value);
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
        Assert.Equal(ForgotPasswordState.TokenValid, sut.State.Value);
    }

    [Fact]
    public void Constructor_StartsInIdleState()
    {
        var sut = new ForgotPasswordViewModel(new FakePasswordRecoveryManager());
        Assert.Equal(ForgotPasswordState.Idle, sut.State.Value);
    }
}