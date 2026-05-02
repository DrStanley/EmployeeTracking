namespace EmployeeTracking.Application.DTOs
{
    public record AuthResponse(
        string Token,
        string Email,
        string FullName,
        IList<string> Roles,
        DateTime ExpiresAt
    );
}
