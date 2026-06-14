using EmployeeTracking.Application.DTOs;
using MediatR;

namespace EmployeeTracking.Application.Queries.Notifications
{
    public record GetMyNotificationsQuery(
    Guid EmployeeId,
    bool UnreadOnly = false
) : IRequest<IReadOnlyList<NotificationDto>>;
}
