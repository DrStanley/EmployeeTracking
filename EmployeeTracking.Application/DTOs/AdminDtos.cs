namespace EmployeeTracking.Application.DTOs
{
    // Attendance Policy 

    public record AttendancePolicyDto(
        Guid Id,
        string Name,
        decimal DailyOvertimeThresholdHours,
        decimal WeeklyOvertimeThresholdHours,
        decimal OvertimeMultiplier,
        bool HasPaidBreaks,
        int PaidBreakMinutes,
        int UnpaidBreakMinutes,
        decimal PTOAccrualRatePerPayPeriod,
        decimal MaxPTOBalanceHours
    );

    public record CreateAttendancePolicyRequest(
        string Name,
        decimal DailyOvertimeThresholdHours,
        decimal WeeklyOvertimeThresholdHours,
        decimal OvertimeMultiplier,
        bool HasPaidBreaks,
        int PaidBreakMinutes,
        int UnpaidBreakMinutes,
        decimal PTOAccrualRatePerPayPeriod,
        decimal MaxPTOBalanceHours
    );

    public record UpdateAttendancePolicyRequest(
        string Name,
        decimal DailyOvertimeThresholdHours,
        decimal WeeklyOvertimeThresholdHours,
        decimal OvertimeMultiplier,
        bool HasPaidBreaks,
        int PaidBreakMinutes,
        int UnpaidBreakMinutes,
        decimal PTOAccrualRatePerPayPeriod,
        decimal MaxPTOBalanceHours
    );

    // Overtime Rule ──

    public record OvertimeRuleDto(
        Guid Id,
        Guid AttendancePolicyId,
        string AttendancePolicyName,
        decimal DailyThresholdHours,
        decimal WeeklyThresholdHours,
        decimal PremiumMultiplier,
        bool HasDoubleTime,
        decimal DoubleTimeDailyThreshold
    );

    public record CreateOvertimeRuleRequest(
        Guid AttendancePolicyId,
        decimal DailyThresholdHours,
        decimal WeeklyThresholdHours,
        decimal PremiumMultiplier,
        bool HasDoubleTime,
        decimal DoubleTimeDailyThreshold
    );

    public record UpdateOvertimeRuleRequest(
        decimal DailyThresholdHours,
        decimal WeeklyThresholdHours,
        decimal PremiumMultiplier,
        bool HasDoubleTime,
        decimal DoubleTimeDailyThreshold
    );

    // Shift 

    public record ShiftDto(
        Guid Id,
        string Name,
        TimeOnly PlannedStart,
        TimeOnly PlannedEnd,
        int GracePeriodMinutes,
        bool IsActive
    );

    public record CreateShiftRequest(
        string Name,
        TimeOnly PlannedStart,
        TimeOnly PlannedEnd,
        int GracePeriodMinutes
    );

    public record UpdateShiftRequest(
        string Name,
        TimeOnly PlannedStart,
        TimeOnly PlannedEnd,
        int GracePeriodMinutes,
        bool IsActive
    );

    // Holiday 

    public record HolidayDto(
        Guid Id,
        string Name,
        DateOnly Date,
        bool IsRecurringAnnually
    );

    public record CreateHolidayRequest(
        string Name,
        DateOnly Date,
        bool IsRecurringAnnually
    );

    public record UpdateHolidayRequest(
        string Name,
        DateOnly Date,
        bool IsRecurringAnnually
    );

    // PTO Accrual Settings 

    public record PTOAccrualSettingsDto(
        Guid AttendancePolicyId,
        string AttendancePolicyName,
        decimal AccrualRatePerPayPeriod,
        decimal MaxBalanceHours,
        bool AllowNegativeBalance,
        int CarryOverLimitHours
    );

    public record UpdatePTOAccrualSettingsRequest(
        decimal AccrualRatePerPayPeriod,
        decimal MaxBalanceHours,
        bool AllowNegativeBalance,
        int CarryOverLimitHours
    );

    // Pay Period 

    public record PayPeriodDto(
        Guid Id,
        string Name,
        DateOnly StartDate,
        DateOnly EndDate,
        bool IsLocked
    );

    public record CreatePayPeriodRequest(
        string Name,
        DateOnly StartDate,
        DateOnly EndDate
    );
}
