using EmployeeTracking.Application.DTOs;
using EmployeeTracking.Application.Interfaces;
using EmployeeTracking.Domain.Entities;
using EmployeeTracking.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace EmployeeTracking.Infrastructure.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IUnitOfWork _uow;
        private readonly IEmailService _emailService;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(
            IUnitOfWork uow,
            IEmailService emailService,
            ILogger<NotificationService> logger)
        {
            _uow = uow;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task SendAsync(
            NotificationPayload payload, CancellationToken ct = default)
        {
            // 1. Persist in-app notification
            var notification = Notification.Create(
                payload.EmployeeId,
                payload.Type,
                payload.Message);

            await _uow.Notifications.AddAsync(notification, ct);
            await _uow.SaveChangesAsync(ct);

            // 2. Send email if employee has an email address
            var employee = await _uow.Employees.GetByIdAsync(payload.EmployeeId, ct);
            if (employee is not null && !string.IsNullOrWhiteSpace(employee.Email))
            {
                var emailMessage = BuildEmailMessage(payload, employee);
                if (emailMessage is not null)
                {
                    var result = await _emailService.SendAsync(emailMessage, ct);
                    if (!result.Success)
                        _logger.LogWarning(
                            "Email notification failed for {EmployeeId}: {Error}",
                            payload.EmployeeId, result.Error);
                }
            }
        }

        public async Task SendBulkAsync(
            IEnumerable<NotificationPayload> payloads, CancellationToken ct = default)
        {
            var emailMessages = new List<(EmailMessages email, bool hasEmail)>();

            foreach (var payload in payloads)
            {
                var notification = Notification.Create(
                    payload.EmployeeId,
                    payload.Type,
                    payload.Message);

                await _uow.Notifications.AddAsync(notification, ct);

                var employee = await _uow.Employees.GetByIdAsync(payload.EmployeeId, ct);
                if (employee is not null && !string.IsNullOrWhiteSpace(employee.Email))
                {
                    var emailMessage = BuildEmailMessage(payload, employee);
                    if (emailMessage is not null)
                        emailMessages.Add((emailMessage, true));
                }
            }

            await _uow.SaveChangesAsync(ct);

            if (emailMessages.Any())
                await _emailService.SendBulkAsync(
                    emailMessages.Select(e => e.email), ct);
        }

        private static EmailMessages? BuildEmailMessage(
            NotificationPayload payload, Employee employee) =>
            payload.Type switch
            {
                NotificationType.MissedPunch =>
                    new EmailMessages(
                        To: employee.Email,
                        Subject: "Action Required — Missed Punch Detected",
                        HtmlBody: EmailTemplates.MissedPunchAlert(
                            employee.FullName, DateTimeOffset.UtcNow.AddHours(-10))),

                NotificationType.PendingApproval =>
                    new EmailMessages(
                        To: employee.Email,
                        Subject: "Timesheet Awaiting Your Approval",
                        HtmlBody: EmailTemplates.TimesheetSubmitted(
                            employee.FullName, "An employee",
                            "current period", Guid.Empty)),

                NotificationType.TimesheetApproved =>
                    new EmailMessages(
                        To: employee.Email,
                        Subject: "Your Timesheet Has Been Approved",
                        HtmlBody: EmailTemplates.TimesheetApproved(
                            employee.FullName, "current period", 40m, 0m)),

                NotificationType.TimesheetRejected =>
                    new EmailMessages(
                        To: employee.Email,
                        Subject: "Your Timesheet Requires Attention",
                        HtmlBody: EmailTemplates.TimesheetRejected(
                            employee.FullName, "current period",
                            payload.Message)),

                NotificationType.OvertimeAlert =>
                    new EmailMessages(
                        To: employee.Email,
                        Subject: "Overtime Alert — Action May Be Required",
                        HtmlBody: EmailTemplates.OvertimeAlert(
                            employee.FullName, 45m, 40m)),

                NotificationType.LowPTOBalance =>
                    new EmailMessages(
                        To: employee.Email,
                        Subject: "Low PTO Balance Reminder",
                        HtmlBody: EmailTemplates.LowPTOBalance(
                            employee.FullName, 8m)),

                _ => null
            };
    }
}
