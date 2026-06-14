using EmployeeTracking.Application.Interfaces;
using EmployeeTracking.Application.Queries.Notifications;
using MediatR;

namespace EmployeeTracking.Infrastructure.Handlers
{

    public class MarkAllNotificationsReadCommandHandler
        : IRequestHandler<MarkAllNotificationsReadCommand, Unit>
    {
        private readonly IUnitOfWork _uow;

        public MarkAllNotificationsReadCommandHandler(IUnitOfWork uow) => _uow = uow;

        public async Task<Unit> Handle(
            MarkAllNotificationsReadCommand request, CancellationToken ct)
        {
            var unread = await _uow.Notifications
                .GetUnreadByEmployeeAsync(request.EmployeeId, ct);

            foreach (var n in unread)
            {
                n.MarkRead();
                _uow.Notifications.Update(n);
            }

            await _uow.SaveChangesAsync(ct);
            return Unit.Value;
        }
    }

}
