using EmployeeTracking.Domain.Enums;

namespace EmployeeTracking.Application.DTOs
{
    /// <summary>Summary of a timesheet for a pay period.</summary>
    public record TimesheetDto(
        Guid Id,
        Guid EmployeeId,
        string EmployeeFullName,
        Guid PayPeriodId,
        string PayPeriodName,
        TimesheetStatus Status,
        decimal TotalRegularHours,
        decimal TotalOvertimeHours,
        decimal TotalPTOHours,
        decimal TotalUnpaidHours,
        DateTimeOffset? SubmittedAt,
        DateTimeOffset? ApprovedAt,
        string? RejectionReason,
        IReadOnlyList<TimesheetLineDto> Lines,
        IReadOnlyList<ApprovalActionDto> ApprovalHistory
    );
}
