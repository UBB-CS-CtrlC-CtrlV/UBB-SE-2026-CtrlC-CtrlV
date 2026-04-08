using BankApp.Contracts.Entities;

namespace BankApp.Contracts.DTOs.Profile;

/// <summary>
/// Represents the response returned when retrieving a user profile.
/// </summary>
public class GetProfileResponse
{
    /// <summary>
    /// Gets or sets a value indicating whether the profile retrieval was successful.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Gets or sets the message describing the result.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the profile information, if retrieval was successful.
    /// </summary>
    public ProfileInfo? ProfileInfo { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="GetProfileResponse"/> class.
    /// </summary>
    public GetProfileResponse()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GetProfileResponse"/> class.
    /// </summary>
    /// <param name="success">Whether the profile retrieval was successful.</param>
    /// <param name="message">A message describing the result.</param>
    public GetProfileResponse(bool success, string message)
    {
        Success = success;
        Message = message;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GetProfileResponse"/> class
    /// and populates the profile info from the given user entity.
    /// </summary>
    /// <param name="success">Whether the profile retrieval was successful.</param>
    /// <param name="message">A message describing the result.</param>
    /// <param name="user">The user entity to extract profile info from.</param>
    public GetProfileResponse(bool success, string message, User user)
    {
        Success = success;
        Message = message;
        ProfileInfo = new ProfileInfo(user);
    }
}