namespace EmployeeTracking.Application.DTOs
{
    /// <summary>Request body for submitting a PTO request.</summary>
    public record SubmitPTORequest(
        /// <summary>First day of the requested time off.</summary>
        DateOnly StartDate,

        /// <summary>Last day of the requested time off.</summary>
        DateOnly EndDate,

        /// <summary>Total hours being requested.</summary>
        decimal HoursRequested,

        /// <summary>Optional note to the manager.</summary>
        string? Notes
    );
}
