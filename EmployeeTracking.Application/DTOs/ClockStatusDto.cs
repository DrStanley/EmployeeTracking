namespace EmployeeTracking.Application.DTOs
{
    public record ClockStatusDto(
        string Status,
        DateTimeOffset? ClockedInAt,
        DateTimeOffset? BreakStartedAt
    );
}
