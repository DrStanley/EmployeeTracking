using EmployeeTracking.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmployeeTracking.Application.Interfaces
{
    public interface INotificationService
    {
        Task SendAsync(NotificationPayload payload, CancellationToken ct = default);
        Task SendBulkAsync(
            IEnumerable<NotificationPayload> payloads, CancellationToken ct = default);
    }
}
