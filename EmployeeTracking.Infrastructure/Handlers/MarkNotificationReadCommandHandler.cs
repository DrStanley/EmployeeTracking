using EmployeeTracking.Application.Interfaces;
using EmployeeTracking.Application.Queries.Notifications;
using EmployeeTracking.Domain.Common;
using MediatR;

namespace EmployeeTracking.Infrastructure.Handlers
{
    public class MarkNotificationReadCommandHandler
        : IRequestHandler<MarkNotificationReadCommand, Unit>
    {
        private readonly IUnitOfWork _uow;

        public MarkNotificationReadCommandHandler(IUnitOfWork uow) => _uow = uow;

        public async Task<Unit> Handle(
            MarkNotificationReadCommand request, CancellationToken ct)
        {
            var notification = await _uow.Notifications
                .GetByIdAsync(request.NotificationId, ct)
                ?? throw new NotFoundException(
                    "Notification", request.NotificationId);

            if (notification.EmployeeId != request.EmployeeId)
                throw new UnauthorizedAccessException(
                    "You can only mark your own notifications as read.");

            notification.MarkRead();
            _uow.Notifications.Update(notification);
            await _uow.SaveChangesAsync(ct);

            return Unit.Value;
        }
    }

}
