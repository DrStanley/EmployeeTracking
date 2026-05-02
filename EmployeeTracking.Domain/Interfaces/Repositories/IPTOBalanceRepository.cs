using EmployeeTracking.Domain.Entities;

namespace EmployeeTracking.Domain.Interfaces.Repositories
{
    public interface IPTOBalanceRepository
    {
        Task<PTOBalance?> GetByEmployeeAndYearAsync(
            Guid employeeId, int year, CancellationToken ct = default);
        Task AddAsync(PTOBalance balance, CancellationToken ct = default);
        void Update(PTOBalance balance);
    }
}
