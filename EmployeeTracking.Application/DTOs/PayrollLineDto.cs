namespace EmployeeTracking.Application.DTOs
{
    /// <summary>Single employee line in a payroll report.</summary>
    public record PayrollLineDto(
        Guid EmployeeId,
        string EmployeeFullName,
        string EmployeeNumber,
        string Department,
        decimal RegularHours,
        decimal OvertimeHours,
        decimal PTOHours,
        decimal UnpaidHours,
        decimal TotalPayableHours,
        bool HasExceptions,
        string? ExceptionNotes,
        string TimesheetStatus
    );
}
