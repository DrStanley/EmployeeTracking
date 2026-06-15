namespace EmployeeTracking.Application.DTOs
{
    public record RevokeTokenRequest(
        string RefreshToken
    );
}
