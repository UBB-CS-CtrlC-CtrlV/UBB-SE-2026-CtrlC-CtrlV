using BankApp.Client.Utilities;
using BankApp.Client.ViewModels;
using BankApp.Client.Enums;
using BankApp.Contracts.DTOs.Profile;
using ErrorOr;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace BankApp.Client.Tests;

/// <summary>
/// Tests for the profile sub-ViewModels: <see cref="ProfileViewModel"/>,
/// <see cref="PersonalInfoViewModel"/>, <see cref="SecurityViewModel"/>,
/// <see cref="OAuthViewModel"/>, and <see cref="NotificationsViewModel"/>.
/// </summary>
public class ProfileViewModelTests
{
    private readonly ApiClient apiClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProfileViewModelTests"/> class.
    /// Creates a fresh substitute for each test.
    /// </summary>
    public ProfileViewModelTests()
    {
        this.apiClient = Substitute.For<ApiClient>(new ConfigurationBuilder().Build(), NullLogger<ApiClient>.Instance);
    }

    // ── PersonalInfoViewModel ──────────────────────────────────────

    /// <summary>
    /// When the API returns a valid profile the ViewModel should populate ProfileInfo.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task LoadProfile_WhenApiReturnsProfile_PopulatesProfileInfo()
    {
        // Arrange
        const int userId = 1;
        const string email = "test@bank.com";
        const string fullName = "Test User";
        const string phoneNumber = "0712345678";
        var vm = new PersonalInfoViewModel(this.apiClient, NullLogger<PersonalInfoViewModel>.Instance);

        this.apiClient.GetAsync<ProfileInfo>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<ErrorOr<ProfileInfo>>(new ProfileInfo
            {
                UserId = userId,
                Email = email,
                FullName = fullName,
                PhoneNumber = phoneNumber,
            }));

        // Act
        bool success = await vm.LoadProfile();

        // Assert
        Assert.True(success);
        Assert.Equal(ProfileState.UpdateSuccess, vm.State.Value);
        Assert.Equal(fullName, vm.ProfileInfo.FullName);
        Assert.Equal(email, vm.ProfileInfo.Email);
    }

    /// <summary>
    /// When the API request fails the ViewModel should enter the error state.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task LoadProfile_WhenApiFails_SetsErrorState()
    {
        // Arrange
        var vm = new PersonalInfoViewModel(this.apiClient, NullLogger<PersonalInfoViewModel>.Instance);

        this.apiClient.GetAsync<ProfileInfo>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<ErrorOr<ProfileInfo>>(Error.Failure(description: "server down")));

        // Act
        bool success = await vm.LoadProfile();

        // Assert
        Assert.False(success);
        Assert.Equal(ProfileState.Error, vm.State.Value);
    }

    /// <summary>
    /// The HasPhoneNumber property should return true when a phone number is set.
    /// </summary>
    [Fact]
    public void HasPhoneNumber_WhenPhoneNumberIsNotSet_ReturnsFalseAndShowsPlaceholder()
    {
        // Arrange
        var vm = new PersonalInfoViewModel(this.apiClient, NullLogger<PersonalInfoViewModel>.Instance);

        // Assert
        Assert.False(vm.HasPhoneNumber);
        Assert.Equal(UserMessages.Profile.NoPhoneNumber, vm.TwoFactorPhoneDisplay);
    }

    /// <summary>
    /// UpdatePersonalInfo should fail when the UserId is missing from ProfileInfo.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task UpdatePersonalInfo_WhenUserIdIsNull_SetsErrorState()
    {
        // Arrange
        const string phoneNumber = "0712345678";
        const string address = "123 Main St";
        const string password = "password";
        var vm = new PersonalInfoViewModel(this.apiClient, NullLogger<PersonalInfoViewModel>.Instance);

        this.apiClient.PutAsync<UpdateProfileRequest>(Arg.Any<string>(), Arg.Any<UpdateProfileRequest>())
            .Returns(Task.FromResult<ErrorOr<Success>>(Result.Success));

        // Act
        bool success = await vm.UpdatePersonalInfo(phoneNumber, address, password);

        // Assert
        Assert.False(success);
        Assert.Equal(ProfileState.Error, vm.State.Value);
    }

    // ── SecurityViewModel ──────────────────────────────────────────

    /// <summary>
    /// ChangePassword should fail when the new password is too short.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task ChangePassword_WhenPasswordTooShort_ReturnsError()
    {
        // Arrange
        const int userId = 1;
        const string currentPassword = "old";
        const string shortPassword = "short";
        var vm = new SecurityViewModel(this.apiClient, NullLogger<SecurityViewModel>.Instance);

        // Act
        var (success, error) = await vm.ChangePassword(userId, currentPassword, shortPassword, shortPassword);

        // Assert
        Assert.False(success);
        Assert.Equal(UserMessages.Security.MinimumLengthRequired, error);
        Assert.Equal(ProfileState.Idle, vm.State.Value);
    }

    /// <summary>
    /// ChangePassword should fail when the new and confirm passwords do not match.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task ChangePassword_WhenPasswordsDoNotMatch_ReturnsError()
    {
        // Arrange
        const int userId = 1;
        const string currentPassword = "old";
        const string newPassword = "LongEnough1!";
        const string confirmPassword = "Different1!";
        var vm = new SecurityViewModel(this.apiClient, NullLogger<SecurityViewModel>.Instance);

        // Act
        var (success, error) = await vm.ChangePassword(userId, currentPassword, newPassword, confirmPassword);

        // Assert
        Assert.False(success);
        Assert.Equal(UserMessages.Security.PasswordMismatch, error);
        Assert.Equal(ProfileState.Idle, vm.State.Value);
    }

    /// <summary>
    /// ChangePassword should succeed when validation passes and the API accepts the request.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task ChangePassword_WhenValid_SetsUpdateSuccessState()
    {
        // Arrange
        const int userId = 1;
        const string currentPassword = "old";
        const string validPassword = "ValidPass1!";
        var vm = new SecurityViewModel(this.apiClient, NullLogger<SecurityViewModel>.Instance);

        this.apiClient.PutAsync<ChangePasswordRequest>(Arg.Any<string>(), Arg.Any<ChangePasswordRequest>())
            .Returns(Task.FromResult<ErrorOr<Success>>(Result.Success));

        // Act
        var (success, error) = await vm.ChangePassword(userId, currentPassword, validPassword, validPassword);

        // Assert
        Assert.True(success);
        Assert.Equal(string.Empty, error);
        Assert.Equal(ProfileState.UpdateSuccess, vm.State.Value);
    }

    /// <summary>
    /// ChangePassword should surface an incorrect-password error from the API.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task ChangePassword_WhenApiReturnsIncorrectPassword_ReturnsSpecificMessage()
    {
        // Arrange
        const int userId = 1;
        const string wrongPassword = "wrong";
        const string validPassword = "ValidPass1!";
        var vm = new SecurityViewModel(this.apiClient, NullLogger<SecurityViewModel>.Instance);

        this.apiClient.PutAsync<ChangePasswordRequest>(Arg.Any<string>(), Arg.Any<ChangePasswordRequest>())
            .Returns(Task.FromResult<ErrorOr<Success>>(Error.Validation(code: "incorrect_password", description: "Wrong password")));

        // Act
        var (success, error) = await vm.ChangePassword(userId, wrongPassword, validPassword, validPassword);

        // Assert
        Assert.False(success);
        Assert.Equal(UserMessages.Security.IncorrectPassword, error);
        Assert.Equal(ProfileState.Error, vm.State.Value);
    }

    /// <summary>
    /// SetTwoFactorEnabled(true) should succeed when the API accepts the request.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task SetTwoFactorEnabled_WhenApiSucceeds_ReturnsTrue()
    {
        // Arrange
        var vm = new SecurityViewModel(this.apiClient, NullLogger<SecurityViewModel>.Instance);

        this.apiClient.PutAsync<object>(Arg.Any<string>(), Arg.Any<object>())
            .Returns(Task.FromResult<ErrorOr<Success>>(Result.Success));

        // Act
        bool result = await vm.SetTwoFactorEnabled(true);

        // Assert
        Assert.True(result);
        Assert.Equal(ProfileState.UpdateSuccess, vm.State.Value);
    }

    /// <summary>
    /// SetTwoFactorEnabled(false) should succeed when the API accepts the request.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task DisableTwoFactor_WhenApiSucceeds_ReturnsTrue()
    {
        // Arrange
        var vm = new SecurityViewModel(this.apiClient, NullLogger<SecurityViewModel>.Instance);

        this.apiClient.PutAsync<object>(Arg.Any<string>(), Arg.Any<object>())
            .Returns(Task.FromResult<ErrorOr<Success>>(Result.Success));

        // Act
        bool result = await vm.SetTwoFactorEnabled(false);

        // Assert
        Assert.True(result);
        Assert.Equal(ProfileState.UpdateSuccess, vm.State.Value);
    }

    /// <summary>
    /// SetTwoFactorEnabled should return false and set error state when the API fails.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task SetTwoFactorEnabled_WhenApiFails_ReturnsFalse()
    {
        // Arrange
        var vm = new SecurityViewModel(this.apiClient, NullLogger<SecurityViewModel>.Instance);

        this.apiClient.PutAsync<object>(Arg.Any<string>(), Arg.Any<object>())
            .Returns(Task.FromResult<ErrorOr<Success>>(Error.Failure(description: "server error")));

        // Act
        bool result = await vm.SetTwoFactorEnabled(true);

        // Assert
        Assert.False(result);
        Assert.Equal(ProfileState.Error, vm.State.Value);
    }

    // ── NotificationsViewModel ─────────────────────────────────────

    /// <summary>
    /// ToggleNotificationPreference should update the preference when the API succeeds.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task ToggleNotificationPreference_WhenApiSucceeds_UpdatesPreference()
    {
        // Arrange
        const int preferenceId = 1;
        var vm = new NotificationsViewModel(this.apiClient, NullLogger<NotificationsViewModel>.Instance);
        var pref = new NotificationPreferenceDto { Id = preferenceId, EmailEnabled = false };
        vm.NotificationPreferences.Add(pref);

        this.apiClient.PutAsync<List<NotificationPreferenceDto>>(Arg.Any<string>(), Arg.Any<List<NotificationPreferenceDto>>())
            .Returns(Task.FromResult<ErrorOr<Success>>(Result.Success));

        // Act
        bool success = await vm.ToggleNotificationPreference(pref, true);

        // Assert
        Assert.True(success);
        Assert.True(pref.EmailEnabled);
        Assert.Equal(ProfileState.UpdateSuccess, vm.State.Value);
    }

    /// <summary>
    /// ToggleNotificationPreference should roll back the preference when the API fails.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task ToggleNotificationPreference_WhenApiFails_RollsBackPreference()
    {
        // Arrange
        const int preferenceId = 1;
        var vm = new NotificationsViewModel(this.apiClient, NullLogger<NotificationsViewModel>.Instance);
        var pref = new NotificationPreferenceDto { Id = preferenceId, EmailEnabled = true };
        vm.NotificationPreferences.Add(pref);

        this.apiClient.PutAsync<List<NotificationPreferenceDto>>(Arg.Any<string>(), Arg.Any<List<NotificationPreferenceDto>>())
            .Returns(Task.FromResult<ErrorOr<Success>>(Error.Failure(description: "server error")));

        // Act
        bool success = await vm.ToggleNotificationPreference(pref, false);

        // Assert
        Assert.False(success);
        Assert.True(pref.EmailEnabled);
    }

    /// <summary>
    /// UpdateNotificationPreferences should return false when the preferences list is empty.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task UpdateNotificationPreferences_WhenListIsEmpty_ReturnsFalse()
    {
        // Arrange
        var vm = new NotificationsViewModel(this.apiClient, NullLogger<NotificationsViewModel>.Instance);

        // Act
        bool result = await vm.UpdateNotificationPreferences(new List<NotificationPreferenceDto>());

        // Assert
        Assert.False(result);
    }

    // ── OAuthViewModel ─────────────────────────────────────────────

    /// <summary>
    /// UnlinkOAuth should remove the provider from the links list.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task UnlinkOAuth_WhenProviderExists_RemovesAndReturnsTrue()
    {
        // Arrange
        const string provider = "Google";
        const string providerEmail = "user@gmail.com";
        var vm = new OAuthViewModel(this.apiClient, NullLogger<OAuthViewModel>.Instance);
        vm.OAuthLinks.Add(new OAuthLinkDto { Provider = provider, ProviderEmail = providerEmail });

        // Act
        bool result = await vm.UnlinkOAuth(provider);

        // Assert
        Assert.True(result);
        Assert.Empty(vm.OAuthLinks);
        Assert.Equal(ProfileState.UpdateSuccess, vm.State.Value);
    }

    /// <summary>
    /// UnlinkOAuth should return false when the provider is not linked.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task UnlinkOAuth_WhenProviderDoesNotExist_ReturnsFalse()
    {
        // Arrange
        const string provider = "GitHub";
        var vm = new OAuthViewModel(this.apiClient, NullLogger<OAuthViewModel>.Instance);

        // Act
        bool result = await vm.UnlinkOAuth(provider);

        // Assert
        Assert.False(result);
    }

    /// <summary>
    /// UnlinkOAuth should return false when provider name is null or whitespace.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task UnlinkOAuth_WhenProviderIsNullOrWhitespace_ReturnsFalse()
    {
        // Arrange
        var vm = new OAuthViewModel(this.apiClient, NullLogger<OAuthViewModel>.Instance);

        // Assert
        Assert.False(await vm.UnlinkOAuth(string.Empty));
        Assert.False(await vm.UnlinkOAuth("  "));
    }

    /// <summary>
    /// LinkOAuth should return false when the provider is already linked.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task LinkOAuth_WhenAlreadyLinked_ReturnsFalse()
    {
        // Arrange
        const string provider = "Google";
        var vm = new OAuthViewModel(this.apiClient, NullLogger<OAuthViewModel>.Instance);
        vm.OAuthLinks.Add(new OAuthLinkDto { Provider = provider });

        // Act
        bool result = await vm.LinkOAuth(provider);

        // Assert
        Assert.False(result);
    }

    // ── ProfileViewModel (coordinator) ─────────────────────────────

    /// <summary>
    /// LoadProfile should set error state when the personal info load fails.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task LoadProfile_WhenPersonalInfoFails_SetsErrorState()
    {
        // Arrange
        this.apiClient.GetAsync<ProfileInfo>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<ErrorOr<ProfileInfo>>(Error.Failure(description: "fail")));

        var profileVm = new ProfileViewModel(
            new PersonalInfoViewModel(this.apiClient, NullLogger<PersonalInfoViewModel>.Instance),
            new SecurityViewModel(this.apiClient, NullLogger<SecurityViewModel>.Instance),
            new OAuthViewModel(this.apiClient, NullLogger<OAuthViewModel>.Instance),
            new NotificationsViewModel(this.apiClient, NullLogger<NotificationsViewModel>.Instance),
            new SessionsViewModel(this.apiClient, NullLogger<SessionsViewModel>.Instance),
            NullLogger<ProfileViewModel>.Instance);

        // Act
        bool success = await profileVm.LoadProfile();

        // Assert
        Assert.False(success);
        Assert.Equal(ProfileState.Error, profileVm.State.Value);
    }

    /// <summary>
    /// The IsInitializingView flag should default to false and be settable.
    /// </summary>
    [Fact]
    public void IsInitializingView_DefaultsFalse_CanBeToggled()
    {
        // Arrange
        var profileVm = new ProfileViewModel(
            new PersonalInfoViewModel(this.apiClient, NullLogger<PersonalInfoViewModel>.Instance),
            new SecurityViewModel(this.apiClient, NullLogger<SecurityViewModel>.Instance),
            new OAuthViewModel(this.apiClient, NullLogger<OAuthViewModel>.Instance),
            new NotificationsViewModel(this.apiClient, NullLogger<NotificationsViewModel>.Instance),
            new SessionsViewModel(this.apiClient, NullLogger<SessionsViewModel>.Instance),
            NullLogger<ProfileViewModel>.Instance);

        // Assert
        Assert.False(profileVm.IsInitializingView);

        // Act
        profileVm.IsInitializingView = true;

        // Assert
        Assert.True(profileVm.IsInitializingView);
    }
}
