namespace EmployeeTracking.Application.DTOs
{
    /// <summary>Response returned after a successful clock-in.</summary>
    public record ClockInResponse(
        /// <summary>The unique ID of the created time entry.</summary>
        Guid EntryId,

        /// <summary>The employee who clocked in.</summary>
        Guid EmployeeId,

        /// <summary>The UTC timestamp of the punch.</summary>
        DateTimeOffset Timestamp,

        /// <summary>Human-readable confirmation message.</summary>
        string Message
    );

}
