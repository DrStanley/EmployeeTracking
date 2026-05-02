namespace EmployeeTracking.Application.DTOs
{
    public record LoginRequest(
        string Email,
        string Password
    );
}
