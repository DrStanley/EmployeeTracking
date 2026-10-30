namespace EmployeeTracking.Application.DTOs
{
    public record WeeklyHoursDto(
        decimal Mon,
        decimal Tue,
        decimal Wed,
        decimal Thu,
        decimal Fri,
        decimal Sat,
        decimal Sun,
        decimal Total);

    public record WorkedHoursDto(decimal Hours);

    public record PresenceDto(
        Guid EmployeeId,
        string FullName,
        string JobTitle,
        string Status,
        DateTimeOffset? Since);

    public record EmployeeProfileDto(
        Guid Id,
        string FullName,
        string Email,
        string JobTitle,
        string? Location,
        string Department,
        string? ManagerName,
        string? ShiftName,
        string? ShiftStart,
        string? ShiftEnd,
        string EmploymentType,
        decimal AvailablePtoHours);

    public record ScheduleDayDto(
        DateOnly Date,
        string DayName,
        bool IsToday,
        bool IsWorkday,
        string? Start,
        string? End);

    public record ScheduleDto(
        string? ShiftName,
        string? ShiftStart,
        string? ShiftEnd,
        IReadOnlyList<ScheduleDayDto> Days);

    public record CalendarHolidayDto(DateOnly Date, string Name);

    public record CalendarAbsenceDto(
        Guid EmployeeId,
        string EmployeeName,
        DateOnly StartDate,
        DateOnly EndDate,
        decimal Hours);

    public record CalendarMonthDto(
        int Year,
        int Month,
        IReadOnlyList<CalendarHolidayDto> Holidays,
        IReadOnlyList<CalendarAbsenceDto> Absences);

    public record AuditLogDto(
        Guid Id,
        string EntityName,
        string EntityId,
        string Action,
        string? Detail,
        string? PerformedByName,
        DateTimeOffset Timestamp);

    public record ForgotPasswordRequest(string Email);

    public record ResetPasswordRequest(string Email, string Token, string NewPassword);
}
