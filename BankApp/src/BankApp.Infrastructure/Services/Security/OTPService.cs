// <copyright file="OtpService.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

using System.Security.Cryptography;
using System.Text;
using BankApp.Application.Services.Security;
using ErrorOr;
using Microsoft.Extensions.Configuration;

namespace BankApp.Infrastructure.Services.Security;

/// <summary>
/// Provides HMAC-based TOTP and in-memory SMS OTP generation and verification.
/// </summary>
public class OtpService : IOtpService
{
    private static readonly Dictionary<int, (string Code, DateTime ExpiryTime)> TemporarySmsStorage = new Dictionary<int, (string Code, DateTime ExpiryTime)>();
    private const int SmsOtpExpiryMinutes = 5;
    private const string FallbackOtpServerSecret = "BankApp-Default-OTP-Secret";
    internal const int TotpWindowSeconds = 60;
    private const int OtpRangeMinimum = 100000;
    private const int OtpRangeMaximum = 999999;
    private const int OtpModulus = 1000000;
    private const int OtpDigitCount = 6;
    private const int TruncationOffsetMask = 0x0F;
    private const int SignBitMask = 0x7F;
    private const int ByteMask = 0xFF;

    private readonly string otpServerSecret;

    /// <summary>
    /// Initializes a new instance of the <see cref="OtpService"/> class.
    /// </summary>
    /// <param name="configuration">The application configuration used to read <c>Otp:ServerSecret</c>.</param>
    public OtpService(IConfiguration configuration)
    {
        this.otpServerSecret = configuration["Otp:ServerSecret"] ?? FallbackOtpServerSecret;
    }

    /// <inheritdoc />
    public ErrorOr<string> GenerateSMSOTP(int userId)
    {
        try
        {
            string code = RandomNumberGenerator.GetInt32(OtpRangeMinimum, OtpRangeMaximum).ToString();
            DateTime expiryTime = DateTime.UtcNow.AddMinutes(SmsOtpExpiryMinutes);
            TemporarySmsStorage[userId] = (code, expiryTime);
            return code;
        }
        catch (Exception ex)
        {
            return Error.Failure(code: "otp.generate_sms_failed", description: ex.Message);
        }
    }

    /// <inheritdoc />
    public ErrorOr<string> GenerateTOTP(int userId)
    {
        try
        {
            long currentWindow = DateTimeOffset.UtcNow.ToUnixTimeSeconds() / TotpWindowSeconds;
            return GenerateHmacCode(userId, currentWindow);
        }
        catch (Exception ex)
        {
            return Error.Failure(code: "otp.generate_totp_failed", description: ex.Message);
        }
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
    public ErrorOr<bool> VerifySMSOTP(int userId, string code)
    {
        try
        {
            if (TemporarySmsStorage.TryGetValue(userId, out (string Code, DateTime ExpiryTime) storedOtpData))
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
        catch (Exception ex)
        {
            return Error.Failure(code: "otp.verify_sms_failed", description: ex.Message);
        }
    }

    /// <inheritdoc />
    public ErrorOr<bool> VerifyTOTP(int userId, string code)
    {
        try
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
        catch (Exception ex)
        {
            return Error.Failure(code: "otp.verify_totp_failed", description: ex.Message);
        }
    }

    private string GenerateHmacCode(int userId, long timeWindow)
    {
        string secret = $"{this.otpServerSecret}_{userId}";
        using HMACSHA1 hmac = new HMACSHA1(Encoding.UTF8.GetBytes(secret));
        byte[] hash = hmac.ComputeHash(BitConverter.GetBytes(timeWindow));
        int offset = hash[hash.Length - 1] & TruncationOffsetMask;
        int binary = ((hash[offset] & SignBitMask) << 24) |
                     ((hash[offset + 1] & ByteMask) << 16) |
                     ((hash[offset + 2] & ByteMask) << 8) |
                     (hash[offset + 3] & ByteMask);
        return (binary % OtpModulus).ToString($"D{OtpDigitCount}");
    }
}
