using System.Text.RegularExpressions;
using System.Net.Mail;

namespace BankApp.Infrastructure.Utilities
{
    /// <summary>
    /// Provides common input validation helper methods.
    /// </summary>
    public static class ValidationUtilities
    {
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
                MailAddress addr = new MailAddress(email);
                return addr.Address == email;
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

            return password.Length >= 8
                && password.Any(char.IsUpper)
                && password.Any(char.IsLower)
                && password.Any(char.IsDigit)
                && password.Any(ch => !char.IsLetterOrDigit(ch));
        }

        /// <summary>
        /// Determines whether the specified string is a valid 6-digit OTP code.
        /// </summary>
        /// <param name="otp">The OTP string to validate.</param>
        /// <returns><see langword="true"/> if the OTP is valid; otherwise, <see langword="false"/>.</returns>
        public static bool IsValidOTP(string otp)
        {
            return !string.IsNullOrWhiteSpace(otp) && otp.Length == 6 && otp.All(char.IsDigit);
        }

        /// <summary>
        /// Determines whether two password strings are equal.
        /// </summary>
        /// <param name="a">The first password.</param>
        /// <param name="b">The second password.</param>
        /// <returns><see langword="true"/> if both passwords are non-null and equal; otherwise, <see langword="false"/>.</returns>
        public static bool PasswordsMatch(string a, string b)
        {
            if (a == null || b == null)
            {
                return false;
            }

            return a == b;
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

            return Regex.IsMatch(phone, @"^\+?[\d\s\-().]{7,15}$");
        }
    }
}

