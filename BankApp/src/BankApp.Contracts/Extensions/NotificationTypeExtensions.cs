using BankApp.Contracts.Enums;

namespace BankApp.Contracts.Extensions;

/// <summary>
/// Provides extension methods for the <see cref="NotificationType"/> enum.
/// </summary>
public static class NotificationTypeExtensions
{
    /// <summary>
    /// Converts a <see cref="NotificationType"/> value to its human-readable display name.
    /// </summary>
    /// <param name="type">The notification type to convert.</param>
    /// <returns>A display-friendly string representation of the notification type.</returns>
    public static string ToDisplayName(this NotificationType type) => type switch
    {
        NotificationType.InboundTransfer => "Inbound Transfer",
        NotificationType.OutboundTransfer => "Outbound Transfer",
        NotificationType.LowBalance => "Low Balance",
        NotificationType.DuePayment => "Due Payment",
        NotificationType.SuspiciousActivity => "Suspicious Activity",
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
        "Payment" => NotificationType.Payment,
        "Inbound Transfer" => NotificationType.InboundTransfer,
        "Outbound Transfer" => NotificationType.OutboundTransfer,
        "Low Balance" => NotificationType.LowBalance,
        "Due Payment" => NotificationType.DuePayment,
        "Suspicious Activity" => NotificationType.SuspiciousActivity,
        _ => throw new ArgumentException($"Unknown NotificationType: {value}")
    };
}