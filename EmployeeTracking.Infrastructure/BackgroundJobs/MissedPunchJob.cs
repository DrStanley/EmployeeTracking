using EmployeeTracking.Application.DTOs;
using EmployeeTracking.Application.Interfaces;
using EmployeeTracking.Domain.Entities;
using EmployeeTracking.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EmployeeTracking.Infrastructure.BackgroundJobs
{
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
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    _logger.LogError(ex, "MissedPunchJob failed.");
                }
            }
        }

        private async Task RunAsync(CancellationToken ct)
        {
            using var scope = _scopeFactory.CreateScope();
            var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var notifications = scope.ServiceProvider.GetRequiredService<INotificationService>();
            var now = DateTimeOffset.UtcNow;
            var today = DateOnly.FromDateTime(now.UtcDateTime);
            var alreadySent = (await uow.Notifications.GetByTypeSinceAsync(
                    NotificationType.MissedPunch, now.AddHours(-20), ct))
                .Select(notification => notification.EmployeeId)
                .ToHashSet();
            var holidays = await uow.Holidays.GetByYearAsync(today.Year, ct);
            var isHoliday = holidays.Any(holiday => OccursOn(holiday, today));
            var onPto = (await uow.PTORequests.GetApprovedOverlappingAsync(today, today, ct))
                .Select(request => request.EmployeeId)
                .ToHashSet();
            var clockedInToday = (await uow.TimeEntries.GetSinceAsync(
                    new DateTimeOffset(today.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero), ct))
                .Where(entry => entry.EntryType == TimeEntryType.ClockIn)
                .Select(entry => entry.EmployeeId)
                .ToHashSet();

            var employees = await uow.Employees.GetAllAsync(ct);
            var payloads = new List<NotificationPayload>();

            foreach (var employee in employees)
            {
                if (alreadySent.Contains(employee.Id))
                    continue;

                var openEntry = await uow.TimeEntries.GetLatestOpenClockInAsync(employee.Id, ct);
                var hasOpenPunch = await uow.TimeEntries.HasOpenClockInAsync(employee.Id, ct);
                if (hasOpenPunch && openEntry is not null &&
                    (now - openEntry.Timestamp).TotalHours >= 10)
                {
                    payloads.Add(new NotificationPayload(
                        employee.Id,
                        NotificationType.MissedPunch,
                        $"You have an open clock-in from {openEntry.Timestamp:MMM dd 'at' HH:mm} UTC with no clock-out."));
                    continue;
                }

                if (isHoliday || today.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
                    continue;
                if (employee.Shift is not { IsActive: true } shift)
                    continue;
                if (now.TimeOfDay < shift.PlannedEnd.ToTimeSpan())
                    continue;
                if (onPto.Contains(employee.Id) || clockedInToday.Contains(employee.Id))
                    continue;

                payloads.Add(new NotificationPayload(
                    employee.Id,
                    NotificationType.MissedPunch,
                    $"You did not clock in for today's {shift.Name} shift ({shift.PlannedStart:HH:mm}–{shift.PlannedEnd:HH:mm})."));
            }

            if (payloads.Count > 0)
                await notifications.SendBulkAsync(payloads, ct);
        }

        private static bool OccursOn(Holiday holiday, DateOnly day)
        {
            if (!holiday.IsRecurringAnnually)
                return holiday.Date == day;
            return holiday.Date.Month == day.Month && holiday.Date.Day == day.Day;
        }
    }
}
