using BankApp.Domain.Entities;

namespace BankApp.Application.DataTransferObjects.Profile;

/// <summary>
/// Represents the profile information of a user.
/// </summary>
public class ProfileInfo
{
    /// <summary>
    /// Gets or sets the user identifier.
    /// </summary>
    public int? UserId { get; set; }

    /// <summary>
    /// Gets or sets the email address.
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Gets or sets the full name.
    /// </summary>
    public string? FullName { get; set; }

    /// <summary>
    /// Gets or sets the phone number.
    /// </summary>
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// Gets or sets the date of birth.
    /// </summary>
    public DateTime? DateOfBirth { get; set; }

    /// <summary>
    /// Gets or sets the address.
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// Gets or sets the nationality.
    /// </summary>
    public string? Nationality { get; set; }

    /// <summary>
    /// Gets or sets the preferred language.
    /// </summary>
    public string? PreferredLanguage { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether two-factor authentication is enabled.
    /// </summary>
    public bool Is2FAEnabled { get; set; }

    /// <summary>
    /// Gets or sets the preferred two-factor authentication method.
    /// </summary>
    public string? Preferred2FAMethod { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ProfileInfo"/> class.
    /// </summary>
    public ProfileInfo()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ProfileInfo"/> class
    /// from a user entity.
    /// </summary>
    /// <param name="user">The user entity to extract profile info from.</param>
    public ProfileInfo(User user)
    {
        if (user != null)
        {
            UserId = user.Id;
            Email = user.Email;
            FullName = user.FullName;
            PhoneNumber = user.PhoneNumber;
            DateOfBirth = user.DateOfBirth;
            Address = user.Address;
            Nationality = user.Nationality;
            PreferredLanguage = user.PreferredLanguage;
            Is2FAEnabled = user.Is2FAEnabled;
            Preferred2FAMethod = user.Preferred2FAMethod;
        }
    }
}
