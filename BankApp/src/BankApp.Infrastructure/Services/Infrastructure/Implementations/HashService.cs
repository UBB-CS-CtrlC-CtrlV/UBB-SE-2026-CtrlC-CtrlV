using BankApp.Infrastructure.Services.Infrastructure.Interfaces;

namespace BankApp.Infrastructure.Services.Infrastructure.Implementations
{
    public class HashService : IHashService
    {
        public string GetHash(string input)
        {
            return BCrypt.Net.BCrypt.HashPassword(input);
        }

        public bool Verify(string input, string hash)
        {
            return BCrypt.Net.BCrypt.Verify(input, hash);
        }
    }
}

