using EmployeeTracking.Domain.Interfaces.Repositories;

namespace EmployeeTracking.Application.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IEmployeeRepository Employees { get; }
        ITimeEntryRepository TimeEntries { get; }
        ITimesheetRepository Timesheets { get; }
        IPTORepository PTORequests { get; }
        IPTOBalanceRepository PTOBalances { get; }
        Task<int> SaveChangesAsync(CancellationToken ct = default);
    }
}
