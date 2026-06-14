using EmployeeTracking.Application.DTOs;
using EmployeeTracking.Application.Interfaces;
using EmployeeTracking.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EmployeeTracking.Infrastructure.BackgroundJobs
{
    /// <summary>
    /// Runs every 30 minutes and checks for employees
    /// who clocked in but never clocked out.
    /// </summary>
    public class MissedPunchJob : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<MissedPunchJob> _logger;
        private readonly TimeSpan _interval = TimeSpan.FromMinutes(30);

        public MissedPunchJob(
            IServiceScopeFactory scopeFactory,
            ILogger<MissedPunchJob> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken ct)
        {
            _logger.LogInformation("MissedPunchJob started.");

            while (!ct.IsCancellationRequested)
            {
                await Task.Delay(_interval, ct);

                try
                {
                    await RunAsync(ct);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "MissedPunchJob failed.");
                }
            }
        }

        private async Task RunAsync(CancellationToken ct)
        {
            using var scope = _scopeFactory.CreateScope();
            var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var notifications = scope.ServiceProvider
                .GetRequiredService<INotificationService>();

            var employees = await uow.Employees.GetAllAsync(ct);
            var payloads = new List<NotificationPayload>();

            foreach (var employee in employees)
            {
                var hasOpenPunch = await uow.TimeEntries
                    .HasOpenClockInAsync(employee.Id, ct);

                if (!hasOpenPunch) continue;

                // Only alert if the open punch is older than 10 hours
                var openEntry = await uow.TimeEntries
                    .GetLatestOpenClockInAsync(employee.Id, ct);

                if (openEntry is null) continue;

                var hoursOpen = (DateTimeOffset.UtcNow - openEntry.Timestamp).TotalHours;
                if (hoursOpen < 10) continue;

                _logger.LogWarning(
                    "Missed punch detected for employee {EmployeeId} — " +
                    "open since {Timestamp}",
                    employee.Id, openEntry.Timestamp);

                payloads.Add(new NotificationPayload(
                    employee.Id,
                    NotificationType.MissedPunch,
                    $"You have an open clock-in from " +
                    $"{openEntry.Timestamp:MMM dd 'at' HH:mm} UTC " +
                    $"with no clock-out. Please contact your manager."));
            }

            if (payloads.Any())
                await notifications.SendBulkAsync(payloads, ct);
        }
    }
}
