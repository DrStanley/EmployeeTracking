using EmployeeTracking.Application.DTOs;
using EmployeeTracking.Application.Interfaces;
using EmployeeTracking.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace EmployeeTracking.Infrastructure.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IUnitOfWork _uow;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(
            IUnitOfWork uow,
            ILogger<NotificationService> logger)
        {
            _uow = uow;
            _logger = logger;
        }

        public async Task SendAsync(
            NotificationPayload payload, CancellationToken ct = default)
        {
            var notification = Notification.Create(
                payload.EmployeeId,
                payload.Type,
                payload.Message);

            await _uow.Notifications.AddAsync(notification, ct);
            await _uow.SaveChangesAsync(ct);

            _logger.LogInformation(
                "Notification sent to employee {EmployeeId}: [{Type}] {Message}",
                payload.EmployeeId, payload.Type, payload.Message);
        }

        public async Task SendBulkAsync(
            IEnumerable<NotificationPayload> payloads,
            CancellationToken ct = default)
        {
            foreach (var payload in payloads)
            {
                var notification = Notification.Create(
                    payload.EmployeeId,
                    payload.Type,
                    payload.Message);

                await _uow.Notifications.AddAsync(notification, ct);
            }

            await _uow.SaveChangesAsync(ct);

            _logger.LogInformation(
                "Bulk notifications sent: {Count}", payloads.Count());
        }
    }
}
