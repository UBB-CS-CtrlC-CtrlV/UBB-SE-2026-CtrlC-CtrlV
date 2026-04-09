// <copyright file="IHashService.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

using ErrorOr;

namespace BankApp.Server.Services.Security;

/// <summary>
/// Defines operations for hashing and verifying values.
/// </summary>
public interface IHashService
{
    /// <summary>
    /// Computes a BCrypt hash of the specified input.
    /// </summary>
    /// <param name="input">The plain-text value to hash.</param>
    /// <returns>
    /// The hashed string on success,
    /// or a failure error if the underlying cryptographic operation throws.
    /// </returns>
    ErrorOr<string> GetHash(string input);

    /// <summary>
    /// Verifies that a plain-text input matches a previously computed BCrypt hash.
    /// </summary>
    /// <param name="input">The plain-text value to verify.</param>
    /// <param name="hash">The BCrypt hash to verify against.</param>
    /// <returns>
    /// <see langword="true"/> if the input matches the hash,
    /// <see langword="false"/> if it does not,
    /// or a failure error if the hash is malformed or the verification throws.
    /// </returns>
    ErrorOr<bool> Verify(string input, string hash);
}
