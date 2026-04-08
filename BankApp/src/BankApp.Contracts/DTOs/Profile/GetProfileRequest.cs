namespace BankApp.Contracts.DTOs.Profile;

/// <summary>
/// Represents a request to retrieve user profile data.
/// This class is currently deprecated as the GetProfile endpoint
/// incorporates its only property, the userId, in the URL.
/// </summary>
[Obsolete("UserId is now passed via the URL. Kept for potential future use.")]
public class GetProfileRequest
{
    /// <summary>
    /// Gets or sets the identifier of the user whose profile is requested.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="GetProfileRequest"/> class.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    public GetProfileRequest(int userId)
    {
        UserId = userId;
    }
}