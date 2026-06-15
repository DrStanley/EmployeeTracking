using EmployeeTracking.Domain.Entities;
using System.Security.Claims;

namespace EmployeeTracking.Application.Interfaces
{
    public interface IJwtTokenService
    {
        string GenerateAccessToken(string userId, string email, Guid employeeId, IList<string> roles);
        RefreshToken GenerateRefreshToken(string userId);

        // Read claims from an expired access token
        // (needed to validate refresh requests)
        ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);

    }
}
