namespace BankApp.Core.DTOs.Profile
{
    public class ChangePasswordRequest
    {
        public int UserId { get; set; }
        public string CurrentPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;

        public ChangePasswordRequest() { }
        
        public ChangePasswordRequest(int userId, string currentPassword, string newPassword)
        {
            UserId = userId;
            CurrentPassword = currentPassword;
            NewPassword = newPassword;
        }   
    }
}

