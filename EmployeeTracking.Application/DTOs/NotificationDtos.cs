using EmployeeTracking.Domain.Enums;

namespace EmployeeTracking.Application.DTOs
{

    /// <summary>A single notification for an employee.</summary>
    public record NotificationDto(
        Guid Id,
        NotificationType Type,
        string Message,
        bool IsRead,
        DateTimeOffset SentAt
    );

    /// <summary>Payload used internally to send a notification.</summary>
    public record NotificationPayload(
        Guid EmployeeId,
        NotificationType Type,
        string Message
    );
}
