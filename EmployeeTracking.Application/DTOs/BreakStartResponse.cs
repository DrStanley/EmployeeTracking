namespace EmployeeTracking.Application.DTOs
{
    public record BreakStartResponse(
        Guid EntryId,
        Guid EmployeeId,
        DateTimeOffset Timestamp,
        string Message
    );
}
