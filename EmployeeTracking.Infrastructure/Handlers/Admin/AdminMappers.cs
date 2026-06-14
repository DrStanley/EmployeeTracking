using EmployeeTracking.Application.DTOs;
using EmployeeTracking.Domain.Entities;

namespace EmployeeTracking.Infrastructure.Handlers.Admin
{
   internal static class AdminMappers
    {
        internal static AttendancePolicyDto MapPolicy(AttendancePolicy p) =>
            new(p.Id, p.Name,
                p.DailyOvertimeThresholdHours,
                p.WeeklyOvertimeThresholdHours,
                p.OvertimeMultiplier,
                p.HasPaidBreaks,
                p.PaidBreakMinutes,
                p.UnpaidBreakMinutes,
                p.PTOAccrualRatePerPayPeriod,
                p.MaxPTOBalanceHours);

        internal static OvertimeRuleDto MapOvertimeRule(OvertimeRule r, string policyName) =>
            new(r.Id, r.AttendancePolicyId, policyName,
                r.DailyThresholdHours,
                r.WeeklyThresholdHours,
                r.PremiumMultiplier,
                r.HasDoubleTime,
                r.DoubleTimeDailyThreshold);

        internal static ShiftDto MapShift(Shift s) =>
            new(s.Id, s.Name, s.PlannedStart, s.PlannedEnd,
                s.GracePeriodMinutes, s.IsActive);

        internal static HolidayDto MapHoliday(Holiday h) =>
            new(h.Id, h.Name, h.Date, h.IsRecurringAnnually);

        internal static PayPeriodDto MapPayPeriod(PayPeriod p) =>
            new(p.Id, p.Name, p.StartDate, p.EndDate, p.IsLocked);
    }
}
