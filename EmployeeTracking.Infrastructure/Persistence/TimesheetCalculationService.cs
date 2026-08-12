using EmployeeTracking.Application.DTOs;
using EmployeeTracking.Application.Interfaces;
using EmployeeTracking.Domain.Entities;
using EmployeeTracking.Domain.Enums;

namespace EmployeeTracking.Infrastructure.Persistence
{
    public class TimesheetCalculationService : ITimesheetCalculationService
    {
        private readonly IUnitOfWork _uow;
        private readonly IOvertimeStrategy _overtimeStrategy;

        public TimesheetCalculationService(
            IUnitOfWork uow,
            IOvertimeStrategy overtimeStrategy)
        {
            _uow = uow;
            _overtimeStrategy = overtimeStrategy;
        }

        public async Task<(decimal regular, decimal overtime, decimal pto, decimal unpaid)>
      CalculateTotalsAsync(
          Guid employeeId,
          PayPeriod period,
          CancellationToken ct = default)
        {
            var from = new DateTimeOffset(
                period.StartDate.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
            var to = new DateTimeOffset(
                period.EndDate.ToDateTime(TimeOnly.MaxValue), TimeSpan.Zero);

            var entries = await _uow.TimeEntries
                .GetByEmployeeAndDateRangeAsync(employeeId, from, to, ct);

            var employee = await _uow.Employees.GetByIdAsync(employeeId, ct);
            var dailyThreshold = employee?.AttendancePolicy?.DailyOvertimeThresholdHours ?? 8m;
            var weeklyThreshold = employee?.AttendancePolicy?.WeeklyOvertimeThresholdHours ?? 40m;

            // Sort all entries by timestamp ascending
            var sorted = entries.OrderBy(e => e.Timestamp).ToList();

            decimal totalWorked = 0m;

            // Pair each ClockIn with the next ClockOut after it
            foreach (var clockIn in sorted.Where(e => e.EntryType == TimeEntryType.ClockIn))
            {
                // Find the first ClockOut that comes after this ClockIn
                var clockOut = sorted.FirstOrDefault(e =>
                    e.EntryType == TimeEntryType.ClockOut &&
                    e.Timestamp > clockIn.Timestamp &&
                    // Make sure it isn't already paired with an earlier ClockIn
                    !sorted.Any(ci =>
                        ci.EntryType == TimeEntryType.ClockIn &&
                        ci.Timestamp > clockIn.Timestamp &&
                        ci.Timestamp < e.Timestamp));

                if (clockOut == null) continue; // open punch — skip

                var workedHours = (decimal)(clockOut.Timestamp - clockIn.Timestamp).TotalHours;

                // Deduct unpaid break time between this clock-in and clock-out
                var breakStarts = sorted.Where(e =>
                    e.EntryType == TimeEntryType.BreakStart &&
                    e.Timestamp > clockIn.Timestamp &&
                    e.Timestamp < clockOut.Timestamp).ToList();

                foreach (var breakStart in breakStarts)
                {
                    var breakEnd = sorted.FirstOrDefault(e =>
                        e.EntryType == TimeEntryType.BreakEnd &&
                        e.Timestamp > breakStart.Timestamp &&
                        e.Timestamp < clockOut.Timestamp);

                    if (breakEnd != null)
                    {
                        var breakHours = (decimal)(breakEnd.Timestamp - breakStart.Timestamp).TotalHours;
                        workedHours -= breakHours;
                    }
                }

                totalWorked += Math.Max(0m, workedHours);
            }

            // Group by day for overtime calculation
            var dailyHours = entries
                .Where(e => e.EntryType == TimeEntryType.ClockIn)
                .GroupBy(e => DateOnly.FromDateTime(e.Timestamp.LocalDateTime))
                .ToDictionary(g => g.Key, g => 0m);

            // Calculate per-day totals using the same pairing logic
            foreach (var clockIn in sorted.Where(e => e.EntryType == TimeEntryType.ClockIn))
            {
                var clockOut = sorted.FirstOrDefault(e =>
                    e.EntryType == TimeEntryType.ClockOut &&
                    e.Timestamp > clockIn.Timestamp &&
                    !sorted.Any(ci =>
                        ci.EntryType == TimeEntryType.ClockIn &&
                        ci.Timestamp > clockIn.Timestamp &&
                        ci.Timestamp < e.Timestamp));

                if (clockOut == null) continue;

                var day = DateOnly.FromDateTime(clockIn.Timestamp.LocalDateTime);
                var worked = (decimal)(clockOut.Timestamp - clockIn.Timestamp).TotalHours;

                if (dailyHours.ContainsKey(day))
                    dailyHours[day] += worked;
                else
                    dailyHours[day] = worked;
            }

            // Apply overtime strategy
            var dailyHoursList = dailyHours
                .Select(d => new DailyHoursDto(d.Key, Math.Max(0m, d.Value)))
                .ToList();

            var (regular, overtime) = _overtimeStrategy.Calculate(
                dailyHoursList, dailyThreshold, weeklyThreshold);

            // Load PTO hours for the period
            var ptoRequests = await _uow.PTORequests.GetByEmployeeAsync(employeeId, ct);
            var ptoHours = ptoRequests
                .Where(p => p.Status == Domain.Enums.PTORequestStatus.Approved
                         && p.StartDate >= period.StartDate
                         && p.EndDate <= period.EndDate)
                .Sum(p => p.HoursRequested);

            return (regular, overtime, ptoHours, 0m);
        }
    }
}
