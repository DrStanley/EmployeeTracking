using System.Security.Claims;

namespace EmployeeTracking.API.Extensions
{

    public static class ClaimsPrincipalExtensions
    {
        public static Guid GetEmployeeId(this ClaimsPrincipal user)
        {
            var value = user.FindFirstValue("employeeId");
            return Guid.TryParse(value, out var id)
                ? id
                : throw new UnauthorizedAccessException("Employee ID claim missing from token.");
        }

        public static string GetRole(this ClaimsPrincipal user)
            => user.FindFirstValue(ClaimTypes.Role) ?? string.Empty;
    }
}
