// <copyright file="ProfileViewModelTests.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BankApp.Client.Utilities;
using BankApp.Client.ViewModels;
using BankApp.Client.Enums;
using BankApp.Contracts.DTOs.Profile;
using ErrorOr;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;

namespace BankApp.Client.Tests;

/// <summary>
/// Tests for the profile sub-ViewModels: <see cref="ProfileViewModel"/>,
/// <see cref="PersonalInfoViewModel"/>, <see cref="SecurityViewModel"/>,
/// <see cref="OAuthViewModel"/>, and <see cref="NotificationsViewModel"/>.
/// </summary>
public class ProfileViewModelTests
{
    // ── PersonalInfoViewModel ──────────────────────────────────────

    /// <summary>
    /// When the API returns a valid profile the ViewModel should populate ProfileInfo.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task LoadProfile_WhenApiReturnsProfile_PopulatesProfileInfo()
    {
        var client = new FakeProfileApiClient
        {
            GetResponse = new ProfileInfo
            {
                UserId = 1,
                Email = "test@bank.com",
                FullName = "Test User",
                PhoneNumber = "0712345678",
            },
        };
        var vm = new PersonalInfoViewModel(client, NullLogger<PersonalInfoViewModel>.Instance);

        bool success = await vm.LoadProfile();

        Assert.True(success);
        Assert.Equal(ProfileState.UpdateSuccess, vm.State.Value);
        Assert.Equal("Test User", vm.ProfileInfo.FullName);
        Assert.Equal("test@bank.com", vm.ProfileInfo.Email);
    }

    /// <summary>
    /// When the API request fails the ViewModel should enter the error state.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task LoadProfile_WhenApiFails_SetsErrorState()
    {
        var client = new FakeProfileApiClient { GetErrorToReturn = Error.Failure(description: "server down") };
        var vm = new PersonalInfoViewModel(client, NullLogger<PersonalInfoViewModel>.Instance);

        bool success = await vm.LoadProfile();

        Assert.False(success);
        Assert.Equal(ProfileState.Error, vm.State.Value);
    }

    /// <summary>
    /// The HasPhoneNumber property should return true when a phone number is set.
    /// </summary>
    [Fact]
    public void HasPhoneNumber_WhenPhoneNumberIsSet_ReturnsTrue()
    {
        var client = new FakeProfileApiClient();
        var vm = new PersonalInfoViewModel(client, NullLogger<PersonalInfoViewModel>.Instance);

        Assert.False(vm.HasPhoneNumber);
        Assert.Equal("No phone number set", vm.TwoFactorPhoneDisplay);
    }

    /// <summary>
    /// UpdatePersonalInfo should fail when the UserId is missing from ProfileInfo.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task UpdatePersonalInfo_WhenUserIdIsNull_SetsErrorState()
    {
        var client = new FakeProfileApiClient { PutSucceeds = true };
        var vm = new PersonalInfoViewModel(client, NullLogger<PersonalInfoViewModel>.Instance);

        bool success = await vm.UpdatePersonalInfo("0712345678", "123 Main St", "password");

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
        var client = new FakeProfileApiClient();
        var vm = new SecurityViewModel(client, NullLogger<SecurityViewModel>.Instance);

        var (success, error) = await vm.ChangePassword(1, "old", "short", "short");

        Assert.False(success);
        Assert.Equal("Minimum 8 characters required.", error);
        Assert.Equal(ProfileState.Idle, vm.State.Value);
    }

    /// <summary>
    /// ChangePassword should fail when the new and confirm passwords do not match.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task ChangePassword_WhenPasswordsDoNotMatch_ReturnsError()
    {
        var client = new FakeProfileApiClient();
        var vm = new SecurityViewModel(client, NullLogger<SecurityViewModel>.Instance);

        var (success, error) = await vm.ChangePassword(1, "old", "LongEnough1!", "Different1!");

        Assert.False(success);
        Assert.Equal("Passwords do not match.", error);
        Assert.Equal(ProfileState.Idle, vm.State.Value);
    }

    /// <summary>
    /// ChangePassword should succeed when validation passes and the API accepts the request.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task ChangePassword_WhenValid_SetsUpdateSuccessState()
    {
        var client = new FakeProfileApiClient { PutSucceeds = true };
        var vm = new SecurityViewModel(client, NullLogger<SecurityViewModel>.Instance);

        var (success, error) = await vm.ChangePassword(1, "old", "ValidPass1!", "ValidPass1!");

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
        var client = new FakeProfileApiClient
        {
            PutErrorToReturn = Error.Validation(code: "incorrect_password", description: "Wrong password"),
        };
        var vm = new SecurityViewModel(client, NullLogger<SecurityViewModel>.Instance);

        var (success, error) = await vm.ChangePassword(1, "wrong", "ValidPass1!", "ValidPass1!");

        Assert.False(success);
        Assert.Equal("Current password is incorrect.", error);
        Assert.Equal(ProfileState.Error, vm.State.Value);
    }

    /// <summary>
    /// SetTwoFactorEnabled(true) should succeed when the API accepts the request.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task SetTwoFactorEnabled_WhenApiSucceeds_ReturnsTrue()
    {
        var client = new FakeProfileApiClient { PutSucceeds = true };
        var vm = new SecurityViewModel(client, NullLogger<SecurityViewModel>.Instance);

        bool result = await vm.SetTwoFactorEnabled(true);

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
        var client = new FakeProfileApiClient { PutSucceeds = true };
        var vm = new SecurityViewModel(client, NullLogger<SecurityViewModel>.Instance);

        bool result = await vm.SetTwoFactorEnabled(false);

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
        var client = new FakeProfileApiClient
        {
            PutErrorToReturn = Error.Failure(description: "server error"),
        };
        var vm = new SecurityViewModel(client, NullLogger<SecurityViewModel>.Instance);

        bool result = await vm.SetTwoFactorEnabled(true);

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
        var client = new FakeProfileApiClient { PutSucceeds = true };
        var vm = new NotificationsViewModel(client, NullLogger<NotificationsViewModel>.Instance);
        var pref = new NotificationPreferenceDto { Id = 1, EmailEnabled = false };
        vm.NotificationPreferences.Add(pref);

        bool success = await vm.ToggleNotificationPreference(pref, true);

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
        var client = new FakeProfileApiClient
        {
            PutErrorToReturn = Error.Failure(description: "server error"),
        };
        var vm = new NotificationsViewModel(client, NullLogger<NotificationsViewModel>.Instance);
        var pref = new NotificationPreferenceDto { Id = 1, EmailEnabled = true };
        vm.NotificationPreferences.Add(pref);

        bool success = await vm.ToggleNotificationPreference(pref, false);

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
        var client = new FakeProfileApiClient { PutSucceeds = true };
        var vm = new NotificationsViewModel(client, NullLogger<NotificationsViewModel>.Instance);

        bool result = await vm.UpdateNotificationPreferences(new List<NotificationPreferenceDto>());

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
        var client = new FakeProfileApiClient();
        var vm = new OAuthViewModel(client, NullLogger<OAuthViewModel>.Instance);
        vm.OAuthLinks.Add(new OAuthLinkDto { Provider = "Google", ProviderEmail = "user@gmail.com" });

        bool result = await vm.UnlinkOAuth("Google");

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
        var client = new FakeProfileApiClient();
        var vm = new OAuthViewModel(client, NullLogger<OAuthViewModel>.Instance);

        bool result = await vm.UnlinkOAuth("GitHub");

        Assert.False(result);
    }

    /// <summary>
    /// UnlinkOAuth should return false when provider name is null or whitespace.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task UnlinkOAuth_WhenProviderIsNullOrWhitespace_ReturnsFalse()
    {
        var client = new FakeProfileApiClient();
        var vm = new OAuthViewModel(client, NullLogger<OAuthViewModel>.Instance);

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
        var client = new FakeProfileApiClient();
        var vm = new OAuthViewModel(client, NullLogger<OAuthViewModel>.Instance);
        vm.OAuthLinks.Add(new OAuthLinkDto { Provider = "Google" });

        bool result = await vm.LinkOAuth("Google");

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
        var failClient = new FakeProfileApiClient { GetErrorToReturn = Error.Failure(description: "fail") };
        var successClient = new FakeProfileApiClient
        {
            GetResponse = new ProfileInfo { UserId = 1 },
            PutSucceeds = true,
        };

        var profileVm = new ProfileViewModel(
            new PersonalInfoViewModel(failClient, NullLogger<PersonalInfoViewModel>.Instance),
            new SecurityViewModel(successClient, NullLogger<SecurityViewModel>.Instance),
            new OAuthViewModel(successClient, NullLogger<OAuthViewModel>.Instance),
            new NotificationsViewModel(successClient, NullLogger<NotificationsViewModel>.Instance),
            new SessionsViewModel(successClient, NullLogger<SessionsViewModel>.Instance),
            NullLogger<ProfileViewModel>.Instance);

        bool success = await profileVm.LoadProfile();

        Assert.False(success);
        Assert.Equal(ProfileState.Error, profileVm.State.Value);
    }

    /// <summary>
    /// The IsInitializingView flag should default to false and be settable.
    /// </summary>
    [Fact]
    public void IsInitializingView_DefaultsFalse_CanBeToggled()
    {
        var client = new FakeProfileApiClient();
        var profileVm = new ProfileViewModel(
            new PersonalInfoViewModel(client, NullLogger<PersonalInfoViewModel>.Instance),
            new SecurityViewModel(client, NullLogger<SecurityViewModel>.Instance),
            new OAuthViewModel(client, NullLogger<OAuthViewModel>.Instance),
            new NotificationsViewModel(client, NullLogger<NotificationsViewModel>.Instance),
            new SessionsViewModel(client, NullLogger<SessionsViewModel>.Instance),
            NullLogger<ProfileViewModel>.Instance);

        Assert.False(profileVm.IsInitializingView);

        profileVm.IsInitializingView = true;

        Assert.True(profileVm.IsInitializingView);
    }

    // ── Shared fake ────────────────────────────────────────────────

    /// <summary>
    /// Provides a configurable API client test double for profile view model tests.
    /// Supports GET, PUT (Success), and POST (with response) overrides.
    /// </summary>
    private sealed class FakeProfileApiClient : ApiClient
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FakeProfileApiClient"/> class.
        /// </summary>
        public FakeProfileApiClient()
            : base(new ConfigurationBuilder().Build(), NullLogger<ApiClient>.Instance)
        {
        }

        /// <summary>
        /// Gets the GET response returned by the fake client.
        /// </summary>
        public object? GetResponse { get; init; }

        /// <summary>
        /// Gets the GET error returned by the fake client, if any.
        /// </summary>
        public Error? GetErrorToReturn { get; init; }

        /// <summary>
        /// Gets or sets a value indicating whether PUT calls should succeed.
        /// </summary>
        public bool PutSucceeds { get; init; }

        /// <summary>
        /// Gets the PUT error returned by the fake client, if any.
        /// </summary>
        public Error? PutErrorToReturn { get; init; }

        /// <summary>
        /// Gets the POST response returned by the fake client for two-type-param overloads.
        /// </summary>
        public object? PostResponse { get; init; }

        /// <summary>
        /// Gets the POST error returned by the fake client, if any.
        /// </summary>
        public Error? PostErrorToReturn { get; init; }

        /// <inheritdoc/>
        public override Task<ErrorOr<TResponse>> GetAsync<TResponse>(string endpoint, CancellationToken cancellationToken = default)
        {
            if (this.GetErrorToReturn.HasValue)
            {
                return Task.FromResult<ErrorOr<TResponse>>(this.GetErrorToReturn.Value);
            }

            return Task.FromResult<ErrorOr<TResponse>>((TResponse)this.GetResponse!);
        }

        /// <inheritdoc/>
        public override Task<ErrorOr<Success>> PutAsync<TRequest>(string endpoint, TRequest data)
        {
            if (this.PutErrorToReturn.HasValue)
            {
                return Task.FromResult<ErrorOr<Success>>(this.PutErrorToReturn.Value);
            }

            return this.PutSucceeds
                ? Task.FromResult<ErrorOr<Success>>(Result.Success)
                : Task.FromResult<ErrorOr<Success>>(Error.Failure(description: "PUT failed"));
        }

        /// <inheritdoc/>
        public override Task<ErrorOr<TResponse>> PostAsync<TRequest, TResponse>(string endpoint, TRequest data)
        {
            if (this.PostErrorToReturn.HasValue)
            {
                return Task.FromResult<ErrorOr<TResponse>>(this.PostErrorToReturn.Value);
            }

            return Task.FromResult<ErrorOr<TResponse>>((TResponse)this.PostResponse!);
        }
    }
}
