using EmployeeTracking.Domain.Enums;

namespace EmployeeTracking.Application.DTOs
{
    public record ClockTodayResponse(
        Guid Id,
        TimeEntryType EntryType,
        TimeEntrySource Source,
        DateTimeOffset Timestamp,
        string? Notes
    );
}
