namespace BankApp.Core.DTOs.Profile
{
    public class ChangePasswordResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }

        public ChangePasswordResponse() {  }

        public ChangePasswordResponse(bool success, string? message)
        {
            Success = success;
            Message = message;
        }
    }
}

