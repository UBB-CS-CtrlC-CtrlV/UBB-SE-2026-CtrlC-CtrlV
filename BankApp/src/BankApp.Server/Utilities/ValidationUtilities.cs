using System.Text.RegularExpressions;
using System.Net.Mail;

namespace BankApp.Server.Utilities;

/// <summary>
/// Provides common input validation helper methods.
/// </summary>
public static class ValidationUtilities
{
    private const int MinPasswordLength = 8;
    private const int OtpCodeLength = 6;
    private const int MinPhoneNumberLength = 7;
    private const int MaxPhoneNumberLength = 15;

    /// <summary>
    /// Determines whether the specified string is a valid email address.
    /// </summary>
    /// <param name="email">The email string to validate.</param>
    /// <returns><see langword="true"/> if the email is valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return false;
        }

        email = email.Trim().ToLower();

        try
        {
            MailAddress mailAddress = new MailAddress(email);
            return mailAddress.Address == email;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Determines whether the specified password meets strength requirements
    /// (at least 8 characters, with uppercase, lowercase, digit, and special character).
    /// </summary>
    /// <param name="password">The password to evaluate.</param>
    /// <returns><see langword="true"/> if the password is strong; otherwise, <see langword="false"/>.</returns>
    public static bool IsStrongPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            return false;
        }

        return password.Length >= MinPasswordLength
               && password.Any(char.IsUpper)
               && password.Any(char.IsLower)
               && password.Any(char.IsDigit)
               && password.Any(character => !char.IsLetterOrDigit(character));
    }

    /// <summary>
    /// Determines whether the specified string is a valid 6-digit OTP code.
    /// </summary>
    /// <param name="otp">The OTP string to validate.</param>
    /// <returns><see langword="true"/> if the OTP is valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValidOTP(string otp)
    {
        return !string.IsNullOrWhiteSpace(otp) && otp.Length == OtpCodeLength && otp.All(char.IsDigit);
    }

    /// <summary>
    /// Determines whether two password strings are equal.
    /// </summary>
    /// <param name="firstPassword">The first password.</param>
    /// <param name="secondPassword">The second password.</param>
    /// <returns><see langword="true"/> if both passwords are non-null and equal; otherwise, <see langword="false"/>.</returns>
    public static bool PasswordsMatch(string firstPassword, string secondPassword)
    {
        if (firstPassword == null || secondPassword == null)
        {
            return false;
        }

        return firstPassword == secondPassword;
    }

    /// <summary>
    /// Determines whether the specified string is a valid phone number (7–15 characters, optional leading +).
    /// </summary>
    /// <param name="phone">The phone number string to validate.</param>
    /// <returns><see langword="true"/> if the phone number is valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValidPhoneNumber(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
        {
            return false;
        }

        return Regex.IsMatch(phone, $@"^\+?[\d\s\-().]{{{MinPhoneNumberLength},{MaxPhoneNumberLength}}}$");
    }
}