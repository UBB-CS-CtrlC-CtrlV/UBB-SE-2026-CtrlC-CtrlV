namespace BankApp.Core.DTOs.Auth
{
    public class VerifyOTPRequest
    {
        public int UserId { get; set; }
        public string OTPCode { get; set; } = string.Empty;
    }
}
