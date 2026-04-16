// <copyright file="NotificationTypeHandlerTests.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

using System.Data;
using System.Diagnostics.CodeAnalysis;
using BankApp.Domain.Enums;
using BankApp.Infrastructure.DataAccess.TypeHandlers;
using FluentAssertions;

namespace BankApp.Infrastructure.Tests.DataAccess.TypeHandlers;

/// <summary>
/// Regression tests for <see cref="NotificationTypeHandler"/>.
///
/// Root cause: the database stores display names with spaces ("Outbound Transfer",
/// "Low Balance", etc.) but the generic EnumTypeHandler was never registered for
/// NotificationType, so Dapper fell back to Enum.Parse — which cannot handle
/// spaced strings and threw on every GET /api/profile/notifications/preferences
/// request where the user had multi-word category rows.
/// </summary>
public class NotificationTypeHandlerTests
{
    private readonly NotificationTypeHandler handler = new NotificationTypeHandler();

    [Theory]
    [InlineData("Payment", NotificationType.Payment)]
    [InlineData("Inbound Transfer", NotificationType.InboundTransfer)]
    [InlineData("Outbound Transfer", NotificationType.OutboundTransfer)]
    [InlineData("Low Balance", NotificationType.LowBalance)]
    [InlineData("Due Payment", NotificationType.DuePayment)]
    [InlineData("Suspicious Activity", NotificationType.SuspiciousActivity)]
    public void Parse_WithDisplayName_ReturnsCorrectEnum(string displayName, NotificationType expected)
    {
        // Act
        NotificationType result = this.handler.Parse(displayName);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(NotificationType.Payment, "Payment")]
    [InlineData(NotificationType.InboundTransfer, "Inbound Transfer")]
    [InlineData(NotificationType.OutboundTransfer, "Outbound Transfer")]
    [InlineData(NotificationType.LowBalance, "Low Balance")]
    [InlineData(NotificationType.DuePayment, "Due Payment")]
    [InlineData(NotificationType.SuspiciousActivity, "Suspicious Activity")]
    public void SetValue_WithEnumValue_SetsDisplayNameOnParameter(NotificationType type, string expectedDisplayName)
    {
        // Arrange
        var parameter = new FakeDbParameter();

        // Act
        this.handler.SetValue(parameter, type);

        // Assert
        parameter.Value.Should().Be(expectedDisplayName);
    }

    [Theory]
    [InlineData(NotificationType.Payment)]
    [InlineData(NotificationType.InboundTransfer)]
    [InlineData(NotificationType.OutboundTransfer)]
    [InlineData(NotificationType.LowBalance)]
    [InlineData(NotificationType.DuePayment)]
    [InlineData(NotificationType.SuspiciousActivity)]
    public void RoundTrip_SetValueThenParse_PreservesOriginalEnum(NotificationType original)
    {
        // Arrange
        var parameter = new FakeDbParameter();

        // Act
        this.handler.SetValue(parameter, original);
        NotificationType roundTripped = this.handler.Parse(parameter.Value!);

        // Assert
        roundTripped.Should().Be(original);
    }

    private sealed class FakeDbParameter : IDbDataParameter
    {
        public object? Value { get; set; }
        public DbType DbType { get; set; }
        public ParameterDirection Direction { get; set; }
        public bool IsNullable => false;
        [AllowNull] public string ParameterName { get; set; } = string.Empty;
        [AllowNull] public string SourceColumn { get; set; } = string.Empty;
        public DataRowVersion SourceVersion { get; set; }
        public byte Precision { get; set; }
        public byte Scale { get; set; }
        public int Size { get; set; }
    }
}
