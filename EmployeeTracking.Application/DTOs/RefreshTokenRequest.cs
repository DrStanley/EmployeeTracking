namespace EmployeeTracking.Application.DTOs
{
    public record RefreshTokenRequest(
    string AccessToken,
    string RefreshToken
);
}
