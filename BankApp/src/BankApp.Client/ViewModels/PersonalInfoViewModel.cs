// <copyright file="PersonalInfoViewModel.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

using System;
using System.Threading.Tasks;
using BankApp.Client.Utilities;
using BankApp.Core.DTOs.Profile;
using BankApp.Client.Enums;
using Microsoft.Extensions.Logging;

namespace BankApp.Client.ViewModels;

/// <summary>
/// Handles loading and updating the user's personal profile information.
/// </summary>
public class PersonalInfoViewModel
{
    private readonly ApiClient apiClient;
    private readonly ILogger<PersonalInfoViewModel> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="PersonalInfoViewModel"/> class.
    /// </summary>
    /// <param name="apiClient">The API client used for profile operations.</param>
    /// <param name="logger">Logger for personal info operation errors.</param>
    public PersonalInfoViewModel(ApiClient apiClient, ILogger<PersonalInfoViewModel> logger)
    {
        this.apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.State = new ObservableState<ProfileState>(ProfileState.Idle);
        this.ProfileInfo = new ProfileInfo();
    }

    /// <summary>
    /// Gets the current profile workflow state.
    /// </summary>
    public ObservableState<ProfileState> State { get; }

    /// <summary>
    /// Gets the current user's profile details.
    /// </summary>
    public ProfileInfo ProfileInfo { get; private set; }

    /// <summary>
    /// Loads the current user's profile information from the server.
    /// </summary>
    /// <returns><see langword="true"/> if the profile loaded successfully; otherwise, <see langword="false"/>.</returns>
    public async Task<bool> LoadProfile()
    {
        this.State.SetValue(ProfileState.Loading);

        var profileResult = await this.apiClient.GetAsync<GetProfileResponse>("api/profile/");
        if (profileResult.IsError)
        {
            this.logger.LogError("LoadProfile: profile request failed: {Errors}", profileResult.Errors);
            this.State.SetValue(ProfileState.Error);
            return false;
        }

        if (!profileResult.Value.Success || profileResult.Value.ProfileInfo == null)
        {
            this.State.SetValue(ProfileState.Error);
            return false;
        }

        this.ProfileInfo = profileResult.Value.ProfileInfo;
        this.State.SetValue(ProfileState.UpdateSuccess);
        return true;
    }

    /// <summary>
    /// Updates the user's phone number and address.
    /// </summary>
    /// <param name="phone">The phone number to persist.</param>
    /// <param name="address">The address to persist.</param>
    /// <param name="password">The verified password associated with the edit flow.</param>
    /// <returns><see langword="true"/> if the update succeeded; otherwise, <see langword="false"/>.</returns>
    public async Task<bool> UpdatePersonalInfo(string? phone, string? address, string password)
    {
        this.State.SetValue(ProfileState.Loading);

        if (this.ProfileInfo.UserId == null)
        {
            this.State.SetValue(ProfileState.Error);
            return false;
        }

        string? trimmedPhone = string.IsNullOrWhiteSpace(phone) ? null : phone.Trim();
        string? trimmedAddress = string.IsNullOrWhiteSpace(address) ? null : address.Trim();

        var request = new UpdateProfileRequest(this.ProfileInfo.UserId, trimmedPhone, trimmedAddress);
        var result = await this.apiClient.PutAsync<UpdateProfileRequest, UpdateProfileResponse>("api/profile/", request);

        return result.Match(
            response =>
            {
                if (!response.Success)
                {
                    this.State.SetValue(ProfileState.Error);
                    return false;
                }

                this.ProfileInfo.PhoneNumber = trimmedPhone;
                this.ProfileInfo.Address = trimmedAddress;
                this.State.SetValue(ProfileState.UpdateSuccess);
                return true;
            },
            errors =>
            {
                this.logger.LogError("UpdatePersonalInfo failed: {Errors}", errors);
                this.State.SetValue(ProfileState.Error);
                return false;
            });
    }

    /// <summary>
    /// Verifies the supplied password against the server.
    /// </summary>
    /// <param name="password">The password to verify.</param>
    /// <returns><see langword="true"/> if the password is valid; otherwise, <see langword="false"/>.</returns>
    public async Task<bool> VerifyPassword(string password)
    {
        this.State.SetValue(ProfileState.Loading);

        if (this.ProfileInfo.UserId == null)
        {
            this.State.SetValue(ProfileState.Error);
            return false;
        }

        var result = await this.apiClient.PostAsync<string, bool>("api/profile/verify-password", password);

        return result.Match(
            valid =>
            {
                if (!valid)
                {
                    this.State.SetValue(ProfileState.Error);
                    return false;
                }

                this.State.SetValue(ProfileState.UpdateSuccess);
                return true;
            },
            errors =>
            {
                this.logger.LogError("VerifyPassword failed: {Errors}", errors);
                this.State.SetValue(ProfileState.Error);
                return false;
            });
    }
}
