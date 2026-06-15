namespace EmployeeTracking.Application.DTOs
{
    public record AuthResponse(
        string AccessToken,
        string RefreshToken,
        string Email,
        string FullName,
        IList<string> Roles,
        DateTime AccessTokenExpiresAt
    );
}
