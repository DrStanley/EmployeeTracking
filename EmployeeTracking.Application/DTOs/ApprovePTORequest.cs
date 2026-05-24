namespace EmployeeTracking.Application.DTOs
{
    /// <summary>Request body for approving a PTO request.</summary>
    public record ApprovePTORequest(
        /// <summary>Optional notes from the reviewer.</summary>
        string? Notes
    );
}
