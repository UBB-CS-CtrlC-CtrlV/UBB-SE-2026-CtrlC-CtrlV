// <copyright file="HashService.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

using BankApp.Application.Services.Security;
using ErrorOr;

namespace BankApp.Infrastructure.Services.Security;

/// <summary>
/// Provides BCrypt-based password hashing and verification.
/// </summary>
public class HashService : IHashService
{
    /// <inheritdoc />
    public ErrorOr<string> GetHash(string input)
    {
        try
        {
            return BCrypt.Net.BCrypt.HashPassword(input);
        }
        catch (Exception exception)
        {
            return Error.Failure(code: "hash.failed", description: exception.Message);
        }
    }

    /// <inheritdoc />
    public ErrorOr<bool> Verify(string input, string hash)
    {
        try
        {
            return BCrypt.Net.BCrypt.Verify(input, hash);
        }
        catch (Exception exception)
        {
            return Error.Failure(code: "hash.verify_failed", description: exception.Message);
        }
    }
}
