namespace EmployeeTracking.Application.DTOs
{
    public record BreakEndResponse(
      Guid EntryId,
      Guid EmployeeId,
      DateTimeOffset Timestamp,
      decimal BreakDurationHours,
      string Message
  );
}
