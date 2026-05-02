using EmployeeTracking.Domain.Enums;

namespace EmployeeTracking.Application.DTOs
{
    /// <summary>A single approval or rejection action on a timesheet.</summary>
    public record ApprovalActionDto(
        Guid ReviewerId,
        string ReviewerFullName,
        ApprovalDecision Decision,
        string? Notes,
        DateTimeOffset DecidedAt
    );
}
