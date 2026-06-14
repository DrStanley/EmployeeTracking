using MediatR;

namespace EmployeeTracking.Application.Queries.Notifications
{
    public record MarkNotificationReadCommand(
        Guid NotificationId,
        Guid EmployeeId
    ) : IRequest<Unit>;
}
