// <copyright file="NotificationTypeHandler.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

using System.Data;
using BankApp.Contracts.Enums;
using BankApp.Contracts.Extensions;
using Dapper;

namespace BankApp.Server.DataAccess.TypeHandlers;

/// <summary>
/// Dapper type handler for <see cref="NotificationType"/>.
/// The database stores display names (e.g. "Outbound Transfer") rather than
/// enum member names, so the generic <see cref="EnumTypeHandler{T}"/> cannot be used.
/// </summary>
public class NotificationTypeHandler : SqlMapper.TypeHandler<NotificationType>
{
    /// <inheritdoc />
    public override NotificationType Parse(object value) =>
        NotificationTypeExtensions.FromString((string)value);

    /// <inheritdoc />
    public override void SetValue(IDbDataParameter parameter, NotificationType value) =>
        parameter.Value = value.ToDisplayName();
}
