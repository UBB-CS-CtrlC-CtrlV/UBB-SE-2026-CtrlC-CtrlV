using BankApp.Client.Utilities;
using BankApp.Client.ViewModels;
using BankApp.Client.Enums;
using BankApp.Contracts.DTOs.Profile;
using ErrorOr;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace BankApp.Client.Tests;

/// <summary>
/// Tests for the profile sub-ViewModels: <see cref="ProfileViewModel"/>,
/// <see cref="PersonalInfoViewModel"/>, <see cref="SecurityViewModel"/>,
/// <see cref="OAuthViewModel"/>, and <see cref="NotificationsViewModel"/>.
/// </summary>
public class ProfileViewModelTests
{
    private readonly Mock<ApiClient> apiClient = new Mock<ApiClient>(
        MockBehavior.Strict,
        new ConfigurationBuilder().Build(),
        NullLogger<ApiClient>.Instance);

    [Fact]
    public async Task LoadProfile_WhenApiReturnsProfile_PopulatesProfileInfo()
    {
        // Arrange
        const int userId = 1;
        const string email = "test@bank.com";
        const string fullName = "Test User";
        const string phoneNumber = "0712345678";
        var vm = new PersonalInfoViewModel(this.apiClient.Object, NullLogger<PersonalInfoViewModel>.Instance);

        this.apiClient.Setup(client =>
                client.GetAsync<ProfileInfo>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ProfileInfo
            {
                UserId = userId,
                Email = email,
                FullName = fullName,
                PhoneNumber = phoneNumber,
            });

        // Act
        bool success = await vm.LoadProfile();

        // Assert
        success.Should().BeTrue();
        vm.State.Value.Should().Be(ProfileState.UpdateSuccess);
        vm.ProfileInfo.FullName.Should().Be(fullName);
        vm.ProfileInfo.Email.Should().Be(email);
    }

    [Fact]
    public async Task LoadProfile_WhenApiFails_SetsErrorState()
    {
        // Arrange
        var vm = new PersonalInfoViewModel(this.apiClient.Object, NullLogger<PersonalInfoViewModel>.Instance);

        this.apiClient.Setup(client =>
                client.GetAsync<ProfileInfo>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Error.Failure(description: "server down"));

        // Act
        bool success = await vm.LoadProfile();

        // Assert
        success.Should().BeFalse();
        vm.State.Value.Should().Be(ProfileState.Error);
    }

    [Fact]
    public void HasPhoneNumber_WhenPhoneNumberIsNotSet_ReturnsFalseAndShowsPlaceholder()
    {
        // Arrange
        var vm = new PersonalInfoViewModel(this.apiClient.Object, NullLogger<PersonalInfoViewModel>.Instance);

        // Assert
        vm.HasPhoneNumber.Should().BeFalse();
        vm.TwoFactorPhoneDisplay.Should().Be(UserMessages.Profile.NoPhoneNumber);
    }

    [Fact]
    public async Task UpdatePersonalInfo_WhenUserIdIsNull_SetsErrorState()
    {
        // Arrange
        const string phoneNumber = "0712345678";
        const string address = "123 Main St";
        const string password = "password";
        var vm = new PersonalInfoViewModel(this.apiClient.Object, NullLogger<PersonalInfoViewModel>.Instance);

        this.apiClient.Setup(client => client.PutAsync(It.IsAny<string>(), It.IsAny<UpdateProfileRequest>()))
            .ReturnsAsync(Result.Success);

        // Act
        bool success = await vm.UpdatePersonalInfo(phoneNumber, address, password);

        // Assert
        success.Should().BeFalse();
        vm.State.Value.Should().Be(ProfileState.Error);
    }

    [Fact]
    public async Task ChangePassword_WhenPasswordTooShort_ReturnsError()
    {
        // Arrange
        const int userId = 1;
        const string currentPassword = "old";
        const string shortPassword = "short";
        var vm = new SecurityViewModel(this.apiClient.Object, NullLogger<SecurityViewModel>.Instance);

        // Act
        (bool success, string error) = await vm.ChangePassword(userId, currentPassword, shortPassword, shortPassword);

        // Assert
        success.Should().BeFalse();
        error.Should().Be(UserMessages.Security.MinimumLengthRequired);
        vm.State.Value.Should().Be(ProfileState.Idle);
    }

    [Fact]
    public async Task ChangePassword_WhenPasswordsDoNotMatch_ReturnsError()
    {
        // Arrange
        const int userId = 1;
        const string currentPassword = "old";
        const string newPassword = "LongEnough1!";
        const string confirmPassword = "Different1!";
        var vm = new SecurityViewModel(this.apiClient.Object, NullLogger<SecurityViewModel>.Instance);

        // Act
        (bool success, string error) = await vm.ChangePassword(userId, currentPassword, newPassword, confirmPassword);

        // Assert
        success.Should().BeFalse();
        error.Should().Be(UserMessages.Security.PasswordMismatch);
        vm.State.Value.Should().Be(ProfileState.Idle);
    }

    [Fact]
    public async Task ChangePassword_WhenValid_SetsUpdateSuccessState()
    {
        // Arrange
        const int userId = 1;
        const string currentPassword = "old";
        const string validPassword = "ValidPass1!";
        var vm = new SecurityViewModel(this.apiClient.Object, NullLogger<SecurityViewModel>.Instance);

        this.apiClient.Setup(client => client.PutAsync(It.IsAny<string>(), It.IsAny<ChangePasswordRequest>()))
            .ReturnsAsync(Result.Success);

        // Act
        (bool success, string error) = await vm.ChangePassword(userId, currentPassword, validPassword, validPassword);

        // Assert
        success.Should().BeTrue();
        error.Should().BeEmpty();
        vm.State.Value.Should().Be(ProfileState.UpdateSuccess);
    }

    [Fact]
    public async Task ChangePassword_WhenApiReturnsIncorrectPassword_ReturnsSpecificMessage()
    {
        // Arrange
        const int userId = 1;
        const string wrongPassword = "wrong";
        const string validPassword = "ValidPass1!";
        var vm = new SecurityViewModel(this.apiClient.Object, NullLogger<SecurityViewModel>.Instance);

        this.apiClient.Setup(client => client.PutAsync(It.IsAny<string>(), It.IsAny<ChangePasswordRequest>()))
            .ReturnsAsync(Error.Validation(code: "incorrect_password", description: "Wrong password"));

        // Act
        (bool success, string error) = await vm.ChangePassword(userId, wrongPassword, validPassword, validPassword);

        // Assert
        success.Should().BeFalse();
        error.Should().Be(UserMessages.Security.IncorrectPassword);
        vm.State.Value.Should().Be(ProfileState.Error);
    }

    [Fact]
    public async Task SetTwoFactorEnabled_WhenApiSucceeds_ReturnsTrue()
    {
        // Arrange
        var vm = new SecurityViewModel(this.apiClient.Object, NullLogger<SecurityViewModel>.Instance);

        this.apiClient.Setup(client => client.PutAsync(It.IsAny<string>(), It.IsAny<object>()))
            .ReturnsAsync(Result.Success);

        // Act
        bool result = await vm.SetTwoFactorEnabled(true);

        // Assert
        result.Should().BeTrue();
        vm.State.Value.Should().Be(ProfileState.UpdateSuccess);
    }

    [Fact]
    public async Task DisableTwoFactor_WhenApiSucceeds_ReturnsTrue()
    {
        // Arrange
        var vm = new SecurityViewModel(this.apiClient.Object, NullLogger<SecurityViewModel>.Instance);

        this.apiClient.Setup(client => client.PutAsync(It.IsAny<string>(), It.IsAny<object>()))
            .ReturnsAsync(Result.Success);

        // Act
        bool result = await vm.SetTwoFactorEnabled(false);

        // Assert
        result.Should().BeTrue();
        vm.State.Value.Should().Be(ProfileState.UpdateSuccess);
    }

    [Fact]
    public async Task SetTwoFactorEnabled_WhenApiFails_ReturnsFalse()
    {
        // Arrange
        var vm = new SecurityViewModel(this.apiClient.Object, NullLogger<SecurityViewModel>.Instance);

        this.apiClient.Setup(client => client.PutAsync(It.IsAny<string>(), It.IsAny<object>()))
            .ReturnsAsync(Error.Failure(description: "server error"));

        // Act
        bool result = await vm.SetTwoFactorEnabled(true);

        // Assert
        result.Should().BeFalse();
        vm.State.Value.Should().Be(ProfileState.Error);
    }

    [Fact]
    public async Task ToggleNotificationPreference_WhenApiSucceeds_UpdatesPreference()
    {
        // Arrange
        const int preferenceId = 1;
        var vm = new NotificationsViewModel(this.apiClient.Object, NullLogger<NotificationsViewModel>.Instance);
        var pref = new NotificationPreferenceDto { Id = preferenceId, EmailEnabled = false };
        vm.NotificationPreferences.Add(pref);

        this.apiClient.Setup(client =>
                client.PutAsync(It.IsAny<string>(), It.IsAny<List<NotificationPreferenceDto>>()))
            .ReturnsAsync(Result.Success);

        // Act
        bool success = await vm.ToggleNotificationPreference(pref, true);

        // Assert
        success.Should().BeTrue();
        pref.EmailEnabled.Should().BeTrue();
        vm.State.Value.Should().Be(ProfileState.UpdateSuccess);
    }

    [Fact]
    public async Task ToggleNotificationPreference_WhenApiFails_RollsBackPreference()
    {
        // Arrange
        const int preferenceId = 1;
        var vm = new NotificationsViewModel(this.apiClient.Object, NullLogger<NotificationsViewModel>.Instance);
        var pref = new NotificationPreferenceDto { Id = preferenceId, EmailEnabled = true };
        vm.NotificationPreferences.Add(pref);

        this.apiClient.Setup(client =>
                client.PutAsync(It.IsAny<string>(), It.IsAny<List<NotificationPreferenceDto>>()))
            .ReturnsAsync(Error.Failure(description: "server error"));

        // Act
        bool success = await vm.ToggleNotificationPreference(pref, false);

        // Assert
        success.Should().BeFalse();
        pref.EmailEnabled.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateNotificationPreferences_WhenListIsEmpty_ReturnsFalse()
    {
        // Arrange
        var vm = new NotificationsViewModel(this.apiClient.Object, NullLogger<NotificationsViewModel>.Instance);

        // Act
        bool result = await vm.UpdateNotificationPreferences(new List<NotificationPreferenceDto>());

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task UnlinkOAuth_WhenProviderExists_RemovesAndReturnsTrue()
    {
        // Arrange
        const string provider = "Google";
        const string providerEmail = "user@gmail.com";
        var vm = new OAuthViewModel(this.apiClient.Object, NullLogger<OAuthViewModel>.Instance);
        vm.OAuthLinks.Add(new OAuthLinkDto { Provider = provider, ProviderEmail = providerEmail });

        // Act
        bool result = await vm.UnlinkOAuth(provider);

        // Assert
        result.Should().BeTrue();
        vm.OAuthLinks.Should().BeEmpty();
        vm.State.Value.Should().Be(ProfileState.UpdateSuccess);
    }

    [Fact]
    public async Task UnlinkOAuth_WhenProviderDoesNotExist_ReturnsFalse()
    {
        // Arrange
        const string provider = "GitHub";
        var vm = new OAuthViewModel(this.apiClient.Object, NullLogger<OAuthViewModel>.Instance);

        // Act
        bool result = await vm.UnlinkOAuth(provider);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task UnlinkOAuth_WhenProviderIsNullOrWhitespace_ReturnsFalse()
    {
        // Arrange
        var vm = new OAuthViewModel(this.apiClient.Object, NullLogger<OAuthViewModel>.Instance);

        // Assert
        (await vm.UnlinkOAuth(string.Empty)).Should().BeFalse();
        (await vm.UnlinkOAuth("  ")).Should().BeFalse();
    }

    [Fact]
    public async Task LinkOAuth_WhenAlreadyLinked_ReturnsFalse()
    {
        // Arrange
        const string provider = "Google";
        var vm = new OAuthViewModel(this.apiClient.Object, NullLogger<OAuthViewModel>.Instance);
        vm.OAuthLinks.Add(new OAuthLinkDto { Provider = provider });

        // Act
        bool result = await vm.LinkOAuth(provider);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task LoadProfile_WhenPersonalInfoFails_SetsErrorState()
    {
        // Arrange
        this.apiClient.Setup(client =>
                client.GetAsync<ProfileInfo>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Error.Failure(description: "fail"));

        var profileVm = new ProfileViewModel(
            new PersonalInfoViewModel(this.apiClient.Object, NullLogger<PersonalInfoViewModel>.Instance),
            new SecurityViewModel(this.apiClient.Object, NullLogger<SecurityViewModel>.Instance),
            new OAuthViewModel(this.apiClient.Object, NullLogger<OAuthViewModel>.Instance),
            new NotificationsViewModel(this.apiClient.Object, NullLogger<NotificationsViewModel>.Instance),
            new SessionsViewModel(this.apiClient.Object, NullLogger<SessionsViewModel>.Instance),
            NullLogger<ProfileViewModel>.Instance);

        // Act
        bool success = await profileVm.LoadProfile();

        // Assert
        success.Should().BeFalse();
        profileVm.State.Value.Should().Be(ProfileState.Error);
    }

    [Fact]
    public void IsInitializingView_DefaultsFalse_CanBeToggled()
    {
        // Arrange
        var profileVm = new ProfileViewModel(
            new PersonalInfoViewModel(this.apiClient.Object, NullLogger<PersonalInfoViewModel>.Instance),
            new SecurityViewModel(this.apiClient.Object, NullLogger<SecurityViewModel>.Instance),
            new OAuthViewModel(this.apiClient.Object, NullLogger<OAuthViewModel>.Instance),
            new NotificationsViewModel(this.apiClient.Object, NullLogger<NotificationsViewModel>.Instance),
            new SessionsViewModel(this.apiClient.Object, NullLogger<SessionsViewModel>.Instance),
            NullLogger<ProfileViewModel>.Instance);

        // Assert initial state
        profileVm.IsInitializingView.Should().BeFalse();

        // Act
        profileVm.IsInitializingView = true;

        // Assert toggled state
        profileVm.IsInitializingView.Should().BeTrue();
    }
}
