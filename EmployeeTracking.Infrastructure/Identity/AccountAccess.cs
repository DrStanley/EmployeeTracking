using EmployeeTracking.Domain.Enums;

namespace EmployeeTracking.Infrastructure.Identity
{
    public static class AccountAccess
    {
        public static AccountStatus Resolve(AccountStatus userStatus, AccountStatus? employeeStatus)
        {
            if (userStatus == AccountStatus.Deleted || employeeStatus == AccountStatus.Deleted)
                return AccountStatus.Deleted;
            if (userStatus == AccountStatus.Suspended || employeeStatus == AccountStatus.Suspended)
                return AccountStatus.Suspended;
            return AccountStatus.Active;
        }
    }
}
