using EmployeeTracking.Application.DTOs;

namespace EmployeeTracking.Application.Interfaces
{

    public interface IEmailService
    {
        Task<EmailResult> SendAsync(EmailMessages message, CancellationToken ct = default);
        Task<EmailResult> SendBulkAsync(
            IEnumerable<EmailMessages> messages, CancellationToken ct = default);
    }
}
