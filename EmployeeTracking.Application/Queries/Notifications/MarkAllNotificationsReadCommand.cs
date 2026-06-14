using MediatR;

namespace EmployeeTracking.Application.Queries.Notifications
{
    public record MarkAllNotificationsReadCommand(
        Guid EmployeeId
    ) : IRequest<Unit>;
}
