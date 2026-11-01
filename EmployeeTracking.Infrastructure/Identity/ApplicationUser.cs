using EmployeeTracking.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace EmployeeTracking.Infrastructure.Identity
{
    public class ApplicationUser : IdentityUser
    {
        public Guid EmployeeId { get; set; }
        public AccountStatus AccountStatus { get; set; } = AccountStatus.Active;
    }
}
