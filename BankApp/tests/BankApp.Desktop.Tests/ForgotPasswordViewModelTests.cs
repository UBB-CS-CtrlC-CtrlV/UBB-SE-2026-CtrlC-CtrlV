using BankApp.Client.Utilities;
using BankApp.Client.ViewModels;
using BankApp.Client.Enums;
using FluentAssertions;
using NSubstitute;

namespace BankApp.Desktop.Tests.ViewModels;

public class ForgotPasswordViewModelTests
{
    private readonly IPasswordRecoveryManager recoveryManager;

    public ForgotPasswordViewModelTests()
    {
        this.recoveryManager = Substitute.For<IPasswordRecoveryManager>();
    }

    [Fact]
    public async Task ForgotPassword_WhenEmailEmpty_SetsErrorState()
    {
        // Arrange
        var vm = new ForgotPasswordViewModel(this.recoveryManager);

        // Act
        await vm.ForgotPassword(string.Empty);

        // Assert
        vm.State.Value.Should().Be(ForgotPasswordState.Error);
        vm.ValidationError.Should().NotBeEmpty();
    }

    [Fact]
    public async Task ForgotPassword_WhenValidEmail_SetsCodeSentState()
    {
        // Arrange
        var vm = new ForgotPasswordViewModel(this.recoveryManager);
        this.recoveryManager.RequestCodeAsync(Arg.Any<string>())
            .Returns(Task.FromResult(ForgotPasswordState.EmailSent));

        // Act
        await vm.ForgotPassword("test@bank.com");

        // Assert
        vm.State.Value.Should().Be(ForgotPasswordState.EmailSent);
        vm.ValidationError.Should().BeEmpty();
    }

    [Fact]
    public async Task ResetPassword_WhenFieldsEmpty_SetsErrorState()
    {
        // Arrange
        var vm = new ForgotPasswordViewModel(this.recoveryManager);

        // Act
        await vm.ResetPassword(string.Empty, string.Empty);

        // Assert
        vm.State.Value.Should().Be(ForgotPasswordState.Error);
        vm.ValidationError.Should().NotBeEmpty();
    }

    [Fact]
    public async Task ResetPassword_WhenPasswordTooWeak_SetsErrorState()
    {
        // Arrange
        var vm = new ForgotPasswordViewModel(this.recoveryManager);
        this.recoveryManager.IsPasswordValid(Arg.Any<string>()).Returns(false);

        // Act
        await vm.ResetPassword("weak", "123456");

        // Assert
        vm.State.Value.Should().Be(ForgotPasswordState.Error);
        vm.ValidationError.Should().NotBeEmpty();
    }

    [Fact]
    public async Task ResetPassword_WhenValidFields_SetsSuccessState()
    {
        // Arrange
        var vm = new ForgotPasswordViewModel(this.recoveryManager);
        this.recoveryManager.IsPasswordValid(Arg.Any<string>()).Returns(true);
        this.recoveryManager.ResetPasswordAsync(Arg.Any<string>(), Arg.Any<string>())
            .Returns(Task.FromResult(ForgotPasswordState.PasswordResetSuccess));

        // Act
        await vm.ResetPassword("StrongP@ss1", "123456");

        // Assert
        vm.State.Value.Should().Be(ForgotPasswordState.PasswordResetSuccess);
        vm.ValidationError.Should().BeEmpty();
    }

    [Fact]
    public async Task VerifyToken_WhenEmpty_SetsErrorState()
    {
        // Arrange
        var vm = new ForgotPasswordViewModel(this.recoveryManager);

        // Act
        await vm.VerifyToken(string.Empty);

        // Assert
        vm.State.Value.Should().Be(ForgotPasswordState.Error);
        vm.ValidationError.Should().NotBeEmpty();
    }

    [Fact]
    public async Task VerifyToken_WhenValid_SetsVerifiedState()
    {
        // Arrange
        var vm = new ForgotPasswordViewModel(this.recoveryManager);
        this.recoveryManager.VerifyTokenAsync(Arg.Any<string>())
            .Returns(Task.FromResult(ForgotPasswordState.TokenValid));

        // Act
        await vm.VerifyToken("123456");

        // Assert
        vm.State.Value.Should().Be(ForgotPasswordState.TokenValid);
        vm.ValidationError.Should().BeEmpty();
    }
}
