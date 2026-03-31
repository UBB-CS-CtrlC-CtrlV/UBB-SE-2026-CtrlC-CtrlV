using BankApp.Core.Enums;

namespace BankApp.Core.DTOs.Profile;

public class Enable2FARequest
{
    public TwoFactorMethod Method { get; set; }
}
