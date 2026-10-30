using EmployeeTracking.Domain.Entities;

namespace EmployeeTracking.Domain.Interfaces.Repositories
{
    public interface IAuditLogRepository
    {
        Task AddAsync(AuditLog log, CancellationToken ct = default);
        Task<IReadOnlyList<AuditLog>> GetRecentAsync(int take, CancellationToken ct = default);
    }
}
