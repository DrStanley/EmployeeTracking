using EmployeeTracking.Application.DTOs;
using EmployeeTracking.Application.Interfaces;
using EmployeeTracking.Domain.Entities;
using Microsoft.Extensions.Logging;
using Resend;

namespace EmployeeTracking.Infrastructure.Services
{
    public class EmailNotificationService : IEmailNotificationService
    {
        private readonly IEmailService _emailService;
        private readonly ILogger<EmailNotificationService> _logger;

        public EmailNotificationService(
            IEmailService emailService,
            ILogger<EmailNotificationService> logger)
        {
            _emailService = emailService;
            _logger = logger;
        }

        public async Task SendTimesheetSubmittedAsync(
            Timesheet timesheet, Employee employee,
            Employee manager, PayPeriod period,
            CancellationToken ct = default)
        {
            var result = await _emailService.SendAsync(new EmailMessages(
                To: manager.Email,
                Subject: $"Timesheet Pending Approval — {employee.FullName}",
                HtmlBody: EmailTemplates.TimesheetSubmitted(
                    manager.FullName,
                    employee.FullName,
                    period.Name,
                    timesheet.Id)), ct);

            LogResult(result, manager.Email, "TimesheetSubmitted");
        }

        public async Task SendTimesheetApprovedAsync(
            Timesheet timesheet, Employee employee,
            PayPeriod period, CancellationToken ct = default)
        {
            var result = await _emailService.SendAsync(new EmailMessages(
                To: employee.Email,
                Subject: $"Timesheet Approved — {period.Name}",
                HtmlBody: EmailTemplates.TimesheetApproved(
                    employee.FullName,
                    period.Name,
                    timesheet.TotalRegularHours,
                    timesheet.TotalOvertimeHours)), ct);

            LogResult(result, employee.Email, "TimesheetApproved");
        }

        public async Task SendTimesheetRejectedAsync(
            Timesheet timesheet, Employee employee,
            PayPeriod period, CancellationToken ct = default)
        {
            var result = await _emailService.SendAsync(new EmailMessages(
                To: employee.Email,
                Subject: $"Timesheet Returned — {period.Name}",
                HtmlBody: EmailTemplates.TimesheetRejected(
                    employee.FullName,
                    period.Name,
                    timesheet.RejectionReason ?? "Please contact your manager.")), ct);

            LogResult(result, employee.Email, "TimesheetRejected");
        }

        public async Task SendPTORequestSubmittedAsync(
            PTORequest request, Employee employee,
            Employee manager, CancellationToken ct = default)
        {
            var result = await _emailService.SendAsync(new EmailMessages(
                To: manager.Email,
                Subject: $"PTO Request Pending — {employee.FullName}",
                HtmlBody: EmailTemplates.PTORequestSubmitted(
                    manager.FullName,
                    employee.FullName,
                    request.StartDate,
                    request.EndDate,
                    request.HoursRequested)), ct);

            LogResult(result, manager.Email, "PTORequestSubmitted");
        }

        public async Task SendPTOApprovedAsync(
            PTORequest request, Employee employee,
            CancellationToken ct = default)
        {
            var result = await _emailService.SendAsync(new EmailMessages(
                To: employee.Email,
                Subject: "PTO Request Approved",
                HtmlBody: EmailTemplates.PTOApproved(
                    employee.FullName,
                    request.StartDate,
                    request.EndDate,
                    request.HoursRequested)), ct);

            LogResult(result, employee.Email, "PTOApproved");
        }

        public async Task SendPTORejectedAsync(
            PTORequest request, Employee employee,
            CancellationToken ct = default)
        {
            var result = await _emailService.SendAsync(new EmailMessages(
                To: employee.Email,
                Subject: "PTO Request Declined",
                HtmlBody: EmailTemplates.PTORejected(
                    employee.FullName,
                    request.StartDate,
                    request.EndDate,
                    request.ReviewerNotes ?? "Please contact your manager.")), ct);

            LogResult(result, employee.Email, "PTORejected");
        }

        public async Task SendWelcomeEmailAsync(
            Employee employee, string temporaryPassword,
            CancellationToken ct = default)
        {
            var result = await _emailService.SendAsync(new EmailMessages(
                To: employee.Email,
                Subject: "Welcome to Employee Tracking System",
                HtmlBody: EmailTemplates.WelcomeEmail(
                    employee.FullName,
                    employee.Email,
                    temporaryPassword)), ct);

            LogResult(result, employee.Email, "WelcomeEmail");
        }

        private void LogResult(EmailResult result, string to, string type)
        {
            if (result.Success)
                _logger.LogInformation(
                    "Email [{Type}] sent to {To} — Id: {Id}", type, to, result.MessageId);
            else
                _logger.LogWarning(
                    "Email [{Type}] failed to {To}: {Error}", type, to, result.Error);
        }
    }
}
