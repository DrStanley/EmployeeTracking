namespace EmployeeTracking.Application.DTOs
{
    public record AuthResponse(
        string Token,
        string Email,
        string FullName,
        string Role,
        DateTime ExpiresAt
    );
}
