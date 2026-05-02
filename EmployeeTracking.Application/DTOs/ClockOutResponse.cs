namespace EmployeeTracking.Application.DTOs
{
    /// <summary>Response returned after a successful clock-out.</summary>
    public record ClockOutResponse(
        /// <summary>The unique ID of the created time entry.</summary>
        Guid EntryId,

        /// <summary>The employee who clocked out.</summary>
        Guid EmployeeId,

        /// <summary>The UTC timestamp of the punch.</summary>
        DateTimeOffset Timestamp,

        /// <summary>Total hours worked since the last clock-in.</summary>
        decimal HoursWorked,

        /// <summary>Human-readable confirmation message.</summary>
        string Message
    );
}
