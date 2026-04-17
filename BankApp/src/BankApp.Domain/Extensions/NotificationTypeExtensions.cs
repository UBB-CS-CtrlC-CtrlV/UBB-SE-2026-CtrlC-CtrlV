using BankApp.Domain.Enums;

namespace BankApp.Domain.Extensions;

/// <summary>
/// Provides extension methods for the <see cref="NotificationType"/> enum.
/// </summary>
public static class NotificationTypeExtensions
{
    private const string PaymentDisplayName = "Payment";
    private const string InboundTransferDisplayName = "Inbound Transfer";
    private const string OutboundTransferDisplayName = "Outbound Transfer";
    private const string LowBalanceDisplayName = "Low Balance";
    private const string DuePaymentDisplayName = "Due Payment";
    private const string SuspiciousActivityDisplayName = "Suspicious Activity";

    /// <summary>
    /// Converts a <see cref="NotificationType"/> value to its human-readable display name.
    /// </summary>
    /// <param name="type">The notification type to convert.</param>
    /// <returns>A display-friendly string representation of the notification type.</returns>
    public static string ToDisplayName(this NotificationType type) => type switch
    {
        NotificationType.InboundTransfer => InboundTransferDisplayName,
        NotificationType.OutboundTransfer => OutboundTransferDisplayName,
        NotificationType.LowBalance => LowBalanceDisplayName,
        NotificationType.DuePayment => DuePaymentDisplayName,
        NotificationType.SuspiciousActivity => SuspiciousActivityDisplayName,
        _ => type.ToString()
    };

    /// <summary>
    /// Converts a display name string to the corresponding <see cref="NotificationType"/> value.
    /// </summary>
    /// <param name="value">The display name string to convert.</param>
    /// <returns>The matching <see cref="NotificationType"/> value.</returns>
    /// <exception cref="ArgumentException">Thrown when the value does not match a known notification type.</exception>
    public static NotificationType FromString(string value) => value switch
    {
        PaymentDisplayName => NotificationType.Payment,
        InboundTransferDisplayName => NotificationType.InboundTransfer,
        OutboundTransferDisplayName => NotificationType.OutboundTransfer,
        LowBalanceDisplayName => NotificationType.LowBalance,
        DuePaymentDisplayName => NotificationType.DuePayment,
        SuspiciousActivityDisplayName => NotificationType.SuspiciousActivity,
        _ => throw new ArgumentException($"Unknown NotificationType: {value}")
    };
}