using EmployeeTracking.Application.Interfaces;
using EmployeeTracking.Domain.Entities;

namespace EmployeeTracking.Infrastructure.Persistence
{
    public class TimesheetCalculationService : ITimesheetCalculationService
    {
        private readonly IUnitOfWork _uow;

        public TimesheetCalculationService(IUnitOfWork uow) => _uow = uow;

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

            decimal totalWorked = 0m;

            // Pair clock-ins with clock-outs
            var clockIns = entries
                .Where(e => e.EntryType == Domain.Enums.TimeEntryType.ClockIn)
                .OrderBy(e => e.Timestamp)
                .ToList();

            var clockOuts = entries
                .Where(e => e.EntryType == Domain.Enums.TimeEntryType.ClockOut)
                .OrderBy(e => e.Timestamp)
                .ToList();

            for (int i = 0; i < Math.Min(clockIns.Count, clockOuts.Count); i++)
            {
                var worked = (decimal)(clockOuts[i].Timestamp - clockIns[i].Timestamp)
                    .TotalHours;
                totalWorked += worked;
            }

            // Apply daily overtime threshold (default 8h/day)
            var employee = await _uow.Employees.GetByIdAsync(employeeId, ct);
            var dailyThreshold = employee?.AttendancePolicy?.DailyOvertimeThresholdHours ?? 8m;
            var weeklyThreshold = employee?.AttendancePolicy?.WeeklyOvertimeThresholdHours ?? 40m;

            decimal regular = Math.Min(totalWorked, weeklyThreshold);
            decimal overtime = Math.Max(0, totalWorked - weeklyThreshold);

            return (regular, overtime, 0m, 0m);
        }
    }
}
