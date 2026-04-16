using BankApp.Server.Utilities;
using FluentAssertions;

namespace BankApp.Server.Tests.Unit.Utilities;

public class ValidationUtilitiesTests
{
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

    [Theory]
    [InlineData("abc", "abc", true)]
    [InlineData("abc", "def", false)]
    [InlineData("", "", true)]
    [InlineData("", "abc", false)]
    [InlineData(null, null, false)]
    [InlineData(null, "abc", false)]
    [InlineData("abc", null, false)]
    public void PasswordsMatch_ShouldReturnExpectedResult(string? firstPassword,
        string? secondPassword,
        bool expected)
    {
        // Act & Assert
        ValidationUtilities.PasswordsMatch(firstPassword, secondPassword).Should().Be(expected);
    }
}