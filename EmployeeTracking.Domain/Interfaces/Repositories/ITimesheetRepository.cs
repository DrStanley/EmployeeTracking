using EmployeeTracking.Domain.Entities;

namespace EmployeeTracking.Domain.Interfaces.Repositories
{

    public interface ITimesheetRepository
    {
        Task<Timesheet?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<Timesheet?> GetByEmployeeAndPeriodAsync(
            Guid employeeId, Guid payPeriodId, CancellationToken ct = default);
        Task<IReadOnlyList<Timesheet>> GetByEmployeeAsync(Guid employeeId, CancellationToken ct = default);
        Task<IReadOnlyList<Timesheet>> GetPendingApprovalsForManagerAsync(
            Guid managerId, CancellationToken ct = default);
        Task AddAsync(Timesheet timesheet, CancellationToken ct = default);
        void Update(Timesheet timesheet);
    }
}
