using BankApp.Application.Utilities;
using FluentAssertions;
using Xunit;

namespace BankApp.Application.Tests.Utilities;

/// <summary>
/// Unit tests for <see cref="ValidationUtilities"/>.
/// </summary>
public class ValidationUtilitiesTests
{
    /// <summary>
    /// Verifies the IsValidEmail_WhenEmailIsInvalid_ShouldReturnFalse scenario.
    /// </summary>
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\n")]
    [InlineData("\r\n")]
    [InlineData("name#provider.com")]
    [InlineData("name@provider")]
    [InlineData("name@.com")]
    [InlineData("@provider.com")]
    [InlineData("name@provider.")]
    [InlineData("name provider@domain.com")]
    [InlineData("name@provider,com")]
    [InlineData("name@@provider.com")]
    [InlineData("name..surname@provider.com")]
    [InlineData(".name@provider.com")]
    [InlineData("name.@provider.com")]
    public void IsValidEmail_WhenEmailIsInvalid_ShouldReturnFalse(string email)
    {
        // Act & Assert
        ValidationUtilities.IsValidEmail(email).Should().BeFalse();
    }

    /// <summary>
    /// Verifies the IsValidEmail_WhenEmailIsValid_ShouldReturnTrue scenario.
    /// </summary>
    [Theory]
    [InlineData("name@provider.com")]
    [InlineData("john.doe@example.org")]
    [InlineData("user123@test.net")]
    [InlineData("first.last@sub.domain.com")]
    [InlineData("a@b.co")]
    [InlineData("name+tag@provider.com")]
    [InlineData("name_surname@provider.com")]
    [InlineData("name-surname@provider.com")]
    public void IsValidEmail_WhenEmailIsValid_ShouldReturnTrue(string email)
    {
        // Act & Assert
        ValidationUtilities.IsValidEmail(email).Should().BeTrue();
    }

    /// <summary>
    /// Verifies the IsStrongPassword_WhenPasswordIsStrong_ShouldReturnTrue scenario.
    /// </summary>
    [Theory]
    [InlineData("Password1!")]
    [InlineData("P@ssw0rd")]
    [InlineData("ComplexPassword123#")]
    public void IsStrongPassword_WhenPasswordIsStrong_ShouldReturnTrue(string password)
    {
        // Act & Assert
        ValidationUtilities.IsStrongPassword(password).Should().BeTrue();
    }

    /// <summary>
    /// Verifies the IsStrongPassword_WhenPasswordIsWeak_ShouldReturnFalse scenario.
    /// </summary>
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("       ")]
    [InlineData("Short1!")]
    [InlineData("password1!")]
    [InlineData("PASSWORD1!")]
    [InlineData("Password!")]
    [InlineData("Password1")]
    public void IsStrongPassword_WhenPasswordIsWeak_ShouldReturnFalse(string password)
    {
        // Act & Assert
        ValidationUtilities.IsStrongPassword(password).Should().BeFalse();
    }

    /// <summary>
    /// Verifies the IsValidOTP_WhenOtpIsValid_ShouldReturnTrue scenario.
    /// </summary>
    [Theory]
    [InlineData("000000")]
    [InlineData("123456")]
    [InlineData("987654")]
    public void IsValidOTP_WhenOtpIsValid_ShouldReturnTrue(string otp)
    {
        // Act & Assert
        ValidationUtilities.IsValidOTP(otp).Should().BeTrue();
    }

    /// <summary>
    /// Verifies the IsValidOTP_WhenOtpIsInvalid_ShouldReturnFalse scenario.
    /// </summary>
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("12345")]
    [InlineData("1234567")]
    [InlineData("12345a")]
    [InlineData("abcdef")]
    public void IsValidOTP_WhenOtpIsInvalid_ShouldReturnFalse(string otp)
    {
        // Act & Assert
        ValidationUtilities.IsValidOTP(otp).Should().BeFalse();
    }

    /// <summary>
    /// Verifies the IsValidPhoneNumber_WhenPhoneNumberIsValid_ShouldReturnTrue scenario.
    /// </summary>
    [Theory]
    [InlineData("0712345678")]
    [InlineData("+40712345678")]
    [InlineData("0040712345678")]
    public void IsValidPhoneNumber_WhenPhoneNumberIsValid_ShouldReturnTrue(string phone)
    {
        // Act & Assert
        ValidationUtilities.IsValidPhoneNumber(phone).Should().BeTrue();
    }

    /// <summary>
    /// Verifies the IsValidPhoneNumber_WhenPhoneNumberIsInvalid_ShouldReturnFalse scenario.
    /// </summary>
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("abc")]
    [InlineData("123")]
    [InlineData("+40")]
    public void IsValidPhoneNumber_WhenPhoneNumberIsInvalid_ShouldReturnFalse(string phone)
    {
        // Act & Assert
        ValidationUtilities.IsValidPhoneNumber(phone).Should().BeFalse();
    }

    /// <summary>
    /// Verifies the NormalizePhoneNumber_WhenPhoneNumberIsValid_ShouldReturnE164Format scenario.
    /// </summary>
    [Theory]
    [InlineData("0712345678", "+40712345678")]
    [InlineData("+40712345678", "+40712345678")]
    [InlineData("0040712345678", "+40712345678")]
    public void NormalizePhoneNumber_WhenPhoneNumberIsValid_ShouldReturnE164Format(string phone, string expected)
    {
        // Act & Assert
        ValidationUtilities.NormalizePhoneNumber(phone).Should().Be(expected);
    }

    /// <summary>
    /// Verifies the NormalizePhoneNumber_WhenPhoneNumberIsInvalid_ShouldReturnNull scenario.
    /// </summary>
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("abc")]
    [InlineData("123")]
    [InlineData("+40")]
    public void NormalizePhoneNumber_WhenPhoneNumberIsInvalid_ShouldReturnNull(string phone)
    {
        // Act & Assert
        ValidationUtilities.NormalizePhoneNumber(phone).Should().BeNull();
    }

    /// <summary>
    /// Verifies the NormalizePhoneNumber_WhenDefaultRegionIsProvided_ShouldUseRegion scenario.
    /// </summary>
    [Fact]
    public void NormalizePhoneNumber_WhenDefaultRegionIsProvided_ShouldUseRegion()
    {
        // Act & Assert
        ValidationUtilities.NormalizePhoneNumber("2025550123", "US").Should().Be("+12025550123");
    }

    /// <summary>
    /// Verifies the PasswordsMatch_ShouldReturnExpectedResult scenario.
    /// </summary>
    [Theory]
    [InlineData("abc", "abc", true)]
    [InlineData("abc", "def", false)]
    [InlineData("", "", true)]
    [InlineData("", "abc", false)]
    [InlineData(null, null, false)]
    [InlineData(null, "abc", false)]
    [InlineData("abc", null, false)]
    public void PasswordsMatch_ShouldReturnExpectedResult(
        string? firstPassword,
        string? secondPassword,
        bool expected)
    {
        // Act & Assert
        ValidationUtilities.PasswordsMatch(firstPassword, secondPassword).Should().Be(expected);
    }
}
