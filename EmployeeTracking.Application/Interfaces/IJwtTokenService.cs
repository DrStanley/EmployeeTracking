namespace EmployeeTracking.Application.Interfaces
{
    public interface IJwtTokenService
    {
        string GenerateToken(string userId, string email, Guid employeeId, IList<string> roles);
    }
}
