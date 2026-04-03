// <copyright file="ApiResponse.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

namespace BankApp.Client;

/// <summary>
/// Represents a simple API response containing either a success message or an error message.
/// </summary>
public class ApiResponse
{
    /// <summary>
    /// Gets or sets the success message returned by the API.
    /// </summary>
    public string? message { get; set; }

    /// <summary>
    /// Gets or sets the error message returned by the API.
    /// </summary>
    public string? error { get; set; }

    /// <summary>
    /// Gets or sets the machine-readable error code returned by the API.
    /// </summary>
    public string? errorCode { get; set; }
}
