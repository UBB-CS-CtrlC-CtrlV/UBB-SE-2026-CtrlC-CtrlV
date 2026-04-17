namespace BankApp.Application.DataTransferObjects.Profile;

/// <summary>
/// Represents a request to update user profile fields.
/// </summary>
public class UpdateProfileRequest
{
    /// <summary>
    /// Gets or sets the identifier of the user whose profile is being updated.
    /// </summary>
    public int? UserId { get; set; }

    /// <summary>
    /// Gets or sets the new full name.
    /// </summary>
    public string? FullName { get; set; }

    /// <summary>
    /// Gets or sets the new phone number.
    /// </summary>
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// Gets or sets the new date of birth.
    /// </summary>
    public DateTime? DateOfBirth { get; set; }

    /// <summary>
    /// Gets or sets the new address.
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// Gets or sets the new nationality.
    /// </summary>
    public string? Nationality { get; set; }

    /// <summary>
    /// Gets or sets the new preferred language.
    /// </summary>
    public string? PreferredLanguage { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateProfileRequest"/> class.
    /// </summary>
    public UpdateProfileRequest()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateProfileRequest"/> class.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="phoneNumber">The new phone number.</param>
    /// <param name="address">The new address.</param>
    public UpdateProfileRequest(int? userId, string? phoneNumber, string? address)
    {
        UserId = userId;
        PhoneNumber = phoneNumber;
        Address = address;
    }
}
