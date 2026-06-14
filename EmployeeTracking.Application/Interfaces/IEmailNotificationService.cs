using EmployeeTracking.Domain.Entities;

namespace EmployeeTracking.Application.Interfaces
{
    public interface IEmailNotificationService
    {
        Task SendTimesheetSubmittedAsync(
            Timesheet timesheet, Employee employee,
            Employee manager, PayPeriod period,
            CancellationToken ct = default);

        Task SendTimesheetApprovedAsync(
            Timesheet timesheet, Employee employee,
            PayPeriod period, CancellationToken ct = default);

        Task SendTimesheetRejectedAsync(
            Timesheet timesheet, Employee employee,
            PayPeriod period, CancellationToken ct = default);

        Task SendPTORequestSubmittedAsync(
            PTORequest request, Employee employee,
            Employee manager, CancellationToken ct = default);

        Task SendPTOApprovedAsync(
            PTORequest request, Employee employee,
            CancellationToken ct = default);

        Task SendPTORejectedAsync(
            PTORequest request, Employee employee,
            CancellationToken ct = default);

        Task SendWelcomeEmailAsync(
            Employee employee, string temporaryPassword,
            CancellationToken ct = default);
    }
}
