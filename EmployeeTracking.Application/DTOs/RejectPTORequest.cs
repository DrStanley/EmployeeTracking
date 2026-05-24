namespace EmployeeTracking.Application.DTOs
{
    /// <summary>Request body for rejecting a PTO request.</summary>
    public record RejectPTORequest(
        /// <summary>Required reason shown to the employee.</summary>
        string Reason
    );
}
