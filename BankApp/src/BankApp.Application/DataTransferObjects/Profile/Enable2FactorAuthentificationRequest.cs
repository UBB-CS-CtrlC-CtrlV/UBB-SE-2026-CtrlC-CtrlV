using BankApp.Domain.Enums;

namespace BankApp.Application.DataTransferObjects.Profile;

/// <summary>
/// Represents a request to enable two-factor authentication.
/// </summary>
public class Enable2FactorAuthentificationRequest
{
    /// <summary>
    /// Gets or sets the two-factor authentication method to enable.
    /// </summary>
    public TwoFactorMethod Method { get; set; }
}