namespace EmployeeTracking.Application.DTOs
{
    /// <summary>Current PTO balance for an employee.</summary>
    public record PTOBalanceDto(
        Guid EmployeeId,
        string EmployeeFullName,
        int Year,
        decimal AvailableHours,
        decimal UsedHours,
        decimal AccruedHours
    );
}
