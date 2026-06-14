using EmployeeTracking.Application.DTOs;
using EmployeeTracking.Application.Interfaces;
using EmployeeTracking.Application.Queries.Notifications;
using MediatR;

namespace EmployeeTracking.Infrastructure.Handlers
{
    public class GetMyNotificationsQueryHandler
    : IRequestHandler<GetMyNotificationsQuery, IReadOnlyList<NotificationDto>>
    {
        private readonly IUnitOfWork _uow;

        public GetMyNotificationsQueryHandler(IUnitOfWork uow) => _uow = uow;

        public async Task<IReadOnlyList<NotificationDto>> Handle(
            GetMyNotificationsQuery request, CancellationToken ct)
        {
            var notifications = request.UnreadOnly
                ? await _uow.Notifications
                    .GetUnreadByEmployeeAsync(request.EmployeeId, ct)
                : await _uow.Notifications
                    .GetByEmployeeAsync(request.EmployeeId, ct);

            return notifications
                .Select(n => new NotificationDto(
                    n.Id,
                    n.Type,
                    n.Message,
                    n.IsRead,
                    n.SentAt))
                .ToList();
        }
    }

}
