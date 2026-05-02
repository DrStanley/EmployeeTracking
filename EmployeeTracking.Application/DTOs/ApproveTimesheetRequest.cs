namespace EmployeeTracking.Application.DTOs
{
    /// <summary>Request body for approving a timesheet.</summary>
    public record ApproveTimesheetRequest(
        /// <summary>Optional notes from the reviewer.</summary>
        string? Notes
    );
}
