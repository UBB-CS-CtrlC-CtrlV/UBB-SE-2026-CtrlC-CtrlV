using BankApp.Client.Enums;
using BankApp.Client.Utilities;
using BankApp.Client.ViewModels;

namespace BankApp.Client.Tests;

public class ForgotPasswordViewModelTests
{
    [Fact]
    public async Task ForgotPassword_EmptyEmail_SetsErrorState()
    {
        var sut = new ForgotPasswordViewModel(new Mock<IPasswordRecoveryManager>().Object);
        await sut.ForgotPassword(string.Empty);
        sut.State.Value.Should().Be(ForgotPasswordState.Error);
    }

    [Fact]
    public async Task ForgotPassword_EmptyEmail_SetsValidationMessage()
    {
        var sut = new ForgotPasswordViewModel(new Mock<IPasswordRecoveryManager>().Object);
        await sut.ForgotPassword(string.Empty);
        sut.ValidationError.Should().Be(UserMessages.ForgotPassword.EmailRequired);
    }

    [Fact]
    public async Task ForgotPassword_ValidEmail_SetsEmailSentState()
    {
        var manager = new Mock<IPasswordRecoveryManager>();
        manager.Setup(m => m.RequestCodeAsync("user@test.com"))
               .ReturnsAsync(ForgotPasswordState.EmailSent);

        var sut = new ForgotPasswordViewModel(manager.Object);
        await sut.ForgotPassword("user@test.com");
        sut.State.Value.Should().Be(ForgotPasswordState.EmailSent);
    }

    [Fact]
    public async Task ResetPassword_EmptyFields_SetsErrorState()
    {
        var sut = new ForgotPasswordViewModel(new Mock<IPasswordRecoveryManager>().Object);
        await sut.ResetPassword(string.Empty, string.Empty);
        sut.State.Value.Should().Be(ForgotPasswordState.Error);
    }

    [Fact]
    public async Task ResetPassword_WeakPassword_SetsErrorState()
    {
        var manager = new Mock<IPasswordRecoveryManager>();
        manager.Setup(m => m.IsPasswordValid(It.IsAny<string>())).Returns(false);

        var sut = new ForgotPasswordViewModel(manager.Object);
        await sut.ResetPassword("weak", "some-token");
        sut.State.Value.Should().Be(ForgotPasswordState.Error);
    }

    [Fact]
    public async Task ResetPassword_ValidInputs_DelegatesToManager()
    {
        var manager = new Mock<IPasswordRecoveryManager>();
        manager.Setup(m => m.IsPasswordValid(It.IsAny<string>())).Returns(true);
        manager.Setup(m => m.ResetPasswordAsync("valid-token", "Password1!"))
               .ReturnsAsync(ForgotPasswordState.PasswordResetSuccess);

        var sut = new ForgotPasswordViewModel(manager.Object);
        await sut.ResetPassword("Password1!", "valid-token");
        sut.State.Value.Should().Be(ForgotPasswordState.PasswordResetSuccess);
    }

    [Fact]
    public async Task VerifyToken_EmptyCode_SetsErrorState()
    {
        var sut = new ForgotPasswordViewModel(new Mock<IPasswordRecoveryManager>().Object);
        await sut.VerifyToken(string.Empty);
        sut.State.Value.Should().Be(ForgotPasswordState.Error);
    }

    [Fact]
    public async Task VerifyToken_ValidCode_DelegatesToManager()
    {
        var manager = new Mock<IPasswordRecoveryManager>();
        manager.Setup(m => m.VerifyTokenAsync("valid-token"))
               .ReturnsAsync(ForgotPasswordState.TokenValid);

        var sut = new ForgotPasswordViewModel(manager.Object);
        await sut.VerifyToken("valid-token");
        sut.State.Value.Should().Be(ForgotPasswordState.TokenValid);
    }

    [Fact]
    public void Constructor_StartsInIdleState()
    {
        var sut = new ForgotPasswordViewModel(new Mock<IPasswordRecoveryManager>().Object);
        sut.State.Value.Should().Be(ForgotPasswordState.Idle);
    }
}