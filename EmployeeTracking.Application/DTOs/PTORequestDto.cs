using EmployeeTracking.Domain.Enums;

namespace EmployeeTracking.Application.DTOs
{
    /// <summary>Full details of a PTO request.</summary>
    public record PTORequestDto(
        Guid Id,
        Guid EmployeeId,
        string EmployeeFullName,
        DateOnly StartDate,
        DateOnly EndDate,
        decimal HoursRequested,
        PTORequestStatus Status,
        string? Notes,
        string? ReviewerNotes,
        string? ReviewedByFullName,
        DateTimeOffset? ReviewedAt,
        DateTimeOffset CreatedAt
    );
}
