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

            // Load all entries for the period
            var entries = await _uow.TimeEntries
                .GetByEmployeeAndDateRangeAsync(employeeId, from, to, ct);

            // Load employee for policy thresholds
            var employee = await _uow.Employees.GetByIdAsync(employeeId, ct);
            var dailyThreshold = employee?.AttendancePolicy?
                                      .DailyOvertimeThresholdHours ?? 8m;
            var weeklyThreshold = employee?.AttendancePolicy?
                                      .WeeklyOvertimeThresholdHours ?? 40m;

            // Build daily hours by pairing clock-ins with clock-outs
            var dailyMap = new Dictionary<DateOnly, decimal>();

            var clockIns = entries
                .Where(e => e.EntryType == TimeEntryType.ClockIn)
                .OrderBy(e => e.Timestamp)
                .ToList();

            var clockOuts = entries
                .Where(e => e.EntryType == TimeEntryType.ClockOut)
                .OrderBy(e => e.Timestamp)
                .ToList();

            for (int i = 0; i < Math.Min(clockIns.Count, clockOuts.Count); i++)
            {
                var date = DateOnly.FromDateTime(clockIns[i].Timestamp.LocalDateTime);
                var worked = (decimal)(clockOuts[i].Timestamp - clockIns[i].Timestamp)
                                  .TotalHours;

                // Deduct unpaid break time
                var unpaidBreaks = entries
                    .Where(e => e.EntryType == TimeEntryType.BreakStart
                             && DateOnly.FromDateTime(e.Timestamp.LocalDateTime) == date)
                    .Sum(b =>
                    {
                        var breakEnd = entries.FirstOrDefault(
                            e => e.EntryType == TimeEntryType.BreakEnd
                              && e.Timestamp > b.Timestamp);
                        return breakEnd is null
                            ? 0m
                            : (decimal)(breakEnd.Timestamp - b.Timestamp).TotalHours;
                    });

                if (dailyMap.ContainsKey(date))
                    dailyMap[date] += worked - unpaidBreaks;
                else
                    dailyMap[date] = worked - unpaidBreaks;
            }

            var dailyHours = dailyMap
                .Select(d => new DailyHoursDto(d.Key, Math.Max(0m, d.Value)))
                .ToList();

            // Load PTO hours for the period
            var ptoRequests = await _uow.PTORequests.GetByEmployeeAsync(employeeId, ct);
            var ptoHours = ptoRequests
                .Where(p => p.Status == Domain.Enums.PTORequestStatus.Approved
                         && p.StartDate >= period.StartDate
                         && p.EndDate <= period.EndDate)
                .Sum(p => p.HoursRequested);

            // Detect missing punches (clock-in with no matching clock-out)
            var unpaidHours = 0m;
            var exceptions = clockIns.Count > clockOuts.Count
                ? "Missing clock-out detected."
                : null;

            // Run overtime calculation
            var (regular, overtime) = _overtimeStrategy.Calculate(
                dailyHours, dailyThreshold, weeklyThreshold);

            return (regular, overtime, ptoHours, unpaidHours);
        }
    }
}
