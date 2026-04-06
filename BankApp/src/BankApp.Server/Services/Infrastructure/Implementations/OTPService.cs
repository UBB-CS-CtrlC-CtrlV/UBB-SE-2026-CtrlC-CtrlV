using System.Security.Cryptography;
using System.Text;
using BankApp.Infrastructure.Services.Infrastructure.Interfaces;

namespace BankApp.Infrastructure.Services.Infrastructure.Implementations
{
    /// <summary>
    /// Provides HMAC-based TOTP and in-memory SMS OTP generation and verification.
    /// </summary>
    public class OtpService : IOtpService
    {
        private static readonly Dictionary<int, (string Code, DateTime ExpiryTime)> TemporarySmsStorage = new
            ();
        private const int SmsOtpExpiryMinutes = 5;
        private const int TotpWindowSeconds = 300;
        private const int OtpRangeMinimum = 100000;
        private const int OtpRangeMaximum = 999999;
        private const int OtpModulus = 1000000;
        private const int OtpDigitCount = 6;
        private const int TruncationOffsetMask = 0x0F;
        private const int SignBitMask = 0x7F;
        private const int ByteMask = 0xFF;

        /// <summary>
        /// Initializes a new instance of the <see cref="OtpService"/> class.
        /// </summary>
        public OtpService()
        {
        }

        /// <inheritdoc />
        public string GenerateSMSOTP(int userId)
        {
            string code = RandomNumberGenerator.GetInt32(OtpRangeMinimum, OtpRangeMaximum).ToString();
            DateTime expiryTime = DateTime.UtcNow.AddMinutes(SmsOtpExpiryMinutes);
            TemporarySmsStorage[userId] = (code, expiryTime);
            return code;
        }

        /// <inheritdoc />
        public string GenerateTOTP(int userId)
        {
            long currentWindow = DateTimeOffset.UtcNow.ToUnixTimeSeconds() / TotpWindowSeconds;
            return GenerateHmacCode(userId, currentWindow);
        }

        /// <inheritdoc />
        public void InvalidateOTP(int userId)
        {
            TemporarySmsStorage.Remove(userId);
        }

        /// <inheritdoc />
        public bool IsExpired(DateTime expiredAt)
        {
            return DateTime.UtcNow > expiredAt;
        }

        /// <inheritdoc />
        public bool VerifySMSOTP(int userId, string code)
        {
            if (TemporarySmsStorage.TryGetValue(userId, out var storedOtpData))
            {
                if (DateTime.UtcNow > storedOtpData.ExpiryTime)
                {
                    InvalidateOTP(userId);
                    return false;
                }
                if (storedOtpData.Code == code)
                {
                    InvalidateOTP(userId);
                    return true;
                }
            }
            return false;
        }

        /// <inheritdoc />
        public bool VerifyTOTP(int userId, string code)
        {
            long currentWindow = DateTimeOffset.UtcNow.ToUnixTimeSeconds() / TotpWindowSeconds;
            if (code == GenerateHmacCode(userId, currentWindow))
            {
                return true;
            }

            if (code == GenerateHmacCode(userId, currentWindow - 1))
            {
                return true;
            }

            return false;
        }

        private string GenerateHmacCode(int userId, long timeWindow)
        {
            string secret = $"User_Secret_Key_{userId}_BankApp";
            using var hmac = new HMACSHA1(Encoding.UTF8.GetBytes(secret));
            byte[] hash = hmac.ComputeHash(BitConverter.GetBytes(timeWindow));
            int offset = hash[hash.Length - 1] & TruncationOffsetMask;
            int binary = ((hash[offset] & SignBitMask) << 24) |
                         ((hash[offset + 1] & ByteMask) << 16) |
                         ((hash[offset + 2] & ByteMask) << 8) |
                         (hash[offset + 3] & ByteMask);
            return (binary % OtpModulus).ToString($"D{OtpDigitCount}");
        }
    }
}

