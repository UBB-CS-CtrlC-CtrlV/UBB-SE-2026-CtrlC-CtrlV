// <copyright file="PasswordValidator.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

using System.Linq;

namespace BankApp.Desktop.Utilities;

/// <summary>
/// Provides password validation rules shared across all client registration and security flows.
/// </summary>
public static class PasswordValidator
{
    /// <summary>
    /// The minimum number of characters required for a valid password.
    /// </summary>
    public const int MinimumLength = 8;

    /// <summary>
    /// Returns <see langword="true"/> when <paramref name="password"/> meets the minimum length requirement.
    /// </summary>
    /// <param name="password">The password to check.</param>
    /// <returns><see langword="true"/> if the password is at least <see cref="MinimumLength"/> characters long.</returns>
    public static bool MeetsMinimumLength(string password)
    {
        return !string.IsNullOrWhiteSpace(password) && password.Length >= MinimumLength;
    }

    /// <summary>
    /// Returns <see langword="true"/> when <paramref name="password"/> satisfies all strength requirements:
    /// minimum length, at least one uppercase letter, one lowercase letter, one digit, and one special character.
    /// </summary>
    /// <param name="password">The password to check.</param>
    /// <returns><see langword="true"/> if the password meets all strength requirements.</returns>
    public static bool IsStrong(string password)
    {
        return MeetsMinimumLength(password)
            && password.Any(char.IsUpper)
            && password.Any(char.IsLower)
            && password.Any(char.IsDigit)
            && password.Any(ch => !char.IsLetterOrDigit(ch));
    }
}
