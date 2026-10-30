using EmployeeTracking.Domain.Entities;
using EmployeeTracking.Domain.Enums;

namespace EmployeeTracking.Domain.Interfaces.Repositories
{
    public interface IAppUsageRepository
    {
        Task AddAsync(AppUsageSession session, CancellationToken ct = default);

        Task<AppUsageSession?> FindDuplicateAsync(
            Guid employeeId,
            string appName,
            DateTimeOffset startedAt,
            DateTimeOffset endedAt,
            AppUsageSource source,
            CancellationToken ct = default);

        Task<IReadOnlyList<AppUsageSession>> GetByEmployeeAndRangeAsync(
            Guid employeeId,
            DateTimeOffset from,
            DateTimeOffset to,
            CancellationToken ct = default);

        Task<IReadOnlyList<AppUsageSession>> GetByEmployeesAndRangeAsync(
            IReadOnlyCollection<Guid> employeeIds,
            DateTimeOffset from,
            DateTimeOffset to,
            CancellationToken ct = default);
    }
}
