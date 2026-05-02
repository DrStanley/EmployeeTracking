namespace EmployeeTracking.Application.DTOs
{
    /// <summary>Request body for rejecting a timesheet.</summary>
    public record RejectTimesheetRequest(
        string Reason
    );
}
