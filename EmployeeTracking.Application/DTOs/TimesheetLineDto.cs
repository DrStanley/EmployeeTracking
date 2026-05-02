namespace EmployeeTracking.Application.DTOs
{
    /// <summary>A single day line on a timesheet.</summary>
    public record TimesheetLineDto(
        Guid Id,
        DateOnly WorkDate,
        decimal RegularHours,
        decimal OvertimeHours,
        decimal BreakHours,
        decimal PTOHours,
        string? Notes
    );
}
