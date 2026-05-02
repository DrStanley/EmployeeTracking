using EmployeeTracking.Domain.Entities;

namespace EmployeeTracking.Domain.Interfaces.Repositories
{
    public interface ITimeEntryRepository
    {
        Task<TimeEntry?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<IReadOnlyList<TimeEntry>> GetByEmployeeAsync(Guid employeeId, CancellationToken ct = default);
        Task<IReadOnlyList<TimeEntry>> GetByEmployeeAndDateRangeAsync(
            Guid employeeId, DateTimeOffset from, DateTimeOffset to, CancellationToken ct = default);
        Task<bool> HasOpenClockInAsync(Guid employeeId, CancellationToken ct = default);
        Task<TimeEntry?> GetLatestOpenClockInAsync(Guid employeeId, CancellationToken ct = default);
        Task AddAsync(TimeEntry entry, CancellationToken ct = default);
        void Update(TimeEntry entry);
    }
}
