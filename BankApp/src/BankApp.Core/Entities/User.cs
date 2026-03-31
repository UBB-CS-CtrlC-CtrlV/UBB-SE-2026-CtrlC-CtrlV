namespace BankApp.Core.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Address { get; set; }
        public string? Nationality { get; set; }
        public string PreferredLanguage { get; set; } = "en";
        public bool Is2FAEnabled { get; set; }
        public string? Preferred2FAMethod { get; set; }
        public bool IsLocked { get; set; }
        public DateTime? LockoutEnd { get; set; }
        public int FailedLoginAttempts { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
