using EmployeeTracking.Application.DTOs;
using EmployeeTracking.Application.Interfaces;
using EmployeeTracking.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EmployeeTracking.Infrastructure.BackgroundJobs
{

    /// <summary>
    /// Runs daily and alerts employees and managers
    /// when weekly hours are approaching or exceeding the overtime threshold.
    /// </summary>
    public class OvertimeAlertJob : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<OvertimeAlertJob> _logger;

        public OvertimeAlertJob(
            IServiceScopeFactory scopeFactory,
            ILogger<OvertimeAlertJob> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken ct)
        {
            _logger.LogInformation("OvertimeAlertJob started.");

            while (!ct.IsCancellationRequested)
            {
                // Run once per day at 6 PM UTC
                var now = DateTime.Today;
                var nextRun = now.Date.AddHours(18);
                if (now >= nextRun) nextRun = nextRun.AddDays(1);

                await Task.Delay(nextRun - now, ct);

                try
                {
                    await RunAsync(ct);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "OvertimeAlertJob failed.");
                }
            }
        }

        private async Task RunAsync(CancellationToken ct)
        {
            using var scope = _scopeFactory.CreateScope();
            var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var notifications = scope.ServiceProvider
                .GetRequiredService<INotificationService>();

            // Get the start of the current week (Monday)
            var today = DateOnly.FromDateTime(DateTime.Now);
            var weekStart = today.AddDays(-(int)today.DayOfWeek + 1);
            var from = new DateTimeOffset(
                weekStart.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
            var to = DateTimeOffset.UtcNow;

            var employees = await uow.Employees.GetAllAsync(ct);
            var payloads = new List<NotificationPayload>();

            foreach (var employee in employees)
            {
                var entries = await uow.TimeEntries
                    .GetByEmployeeAndDateRangeAsync(employee.Id, from, to, ct);

                var clockIns = entries
                    .Where(e => e.EntryType == TimeEntryType.ClockIn)
                    .OrderBy(e => e.Timestamp).ToList();

                var clockOuts = entries
                    .Where(e => e.EntryType == TimeEntryType.ClockOut)
                    .OrderBy(e => e.Timestamp).ToList();

                decimal totalHours = 0m;
                for (int i = 0; i < Math.Min(clockIns.Count, clockOuts.Count); i++)
                {
                    totalHours += (decimal)(clockOuts[i].Timestamp
                        - clockIns[i].Timestamp).TotalHours;
                }

                var threshold = employee.AttendancePolicy?
                    .WeeklyOvertimeThresholdHours ?? 40m;

                // Alert at 80% of threshold (e.g. 32h) and again when exceeded
                if (totalHours >= threshold)
                {
                    payloads.Add(new NotificationPayload(
                        employee.Id,
                        NotificationType.OvertimeAlert,
                        $"You have worked {totalHours:F1}h this week, " +
                        $"exceeding the {threshold}h overtime threshold. " +
                        "Your manager has been notified."));

                    // Also notify manager
                    if (employee.ManagerId.HasValue)
                        payloads.Add(new NotificationPayload(
                            employee.ManagerId.Value,
                            NotificationType.OvertimeAlert,
                            $"{employee.FullName} has worked {totalHours:F1}h " +
                            $"this week, exceeding the {threshold}h threshold."));
                }
                else if (totalHours >= threshold * 0.8m)
                {
                    payloads.Add(new NotificationPayload(
                        employee.Id,
                        NotificationType.OvertimeAlert,
                        $"Heads up — you have worked {totalHours:F1}h this week " +
                        $"and are approaching the {threshold}h overtime threshold."));
                }
            }

            if (payloads.Any())
                await notifications.SendBulkAsync(payloads, ct);

            _logger.LogInformation(
                "OvertimeAlertJob completed — {Count} alerts sent.", payloads.Count);
        }
    }
}
