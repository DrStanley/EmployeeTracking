using EmployeeTracking.Domain.Entities;
using EmployeeTracking.Domain.Enums;
using EmployeeTracking.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EmployeeTracking.Infrastructure.Persistence.Repositories
{
    public class AppUsageRepository : IAppUsageRepository
    {
        private readonly AppDbContext _context;

        public AppUsageRepository(AppDbContext context) => _context = context;

        public async Task AddAsync(AppUsageSession session, CancellationToken ct = default)
            => await _context.AppUsageSessions.AddAsync(session, ct);

        public async Task<AppUsageSession?> FindDuplicateAsync(
            Guid employeeId,
            string appName,
            DateTimeOffset startedAt,
            DateTimeOffset endedAt,
            AppUsageSource source,
            CancellationToken ct = default)
            => await _context.AppUsageSessions.FirstOrDefaultAsync(s =>
                s.EmployeeId == employeeId
                && s.AppName == appName
                && s.StartedAt == startedAt
                && s.EndedAt == endedAt
                && s.Source == source, ct);

        public async Task<IReadOnlyList<AppUsageSession>> GetByEmployeeAndRangeAsync(
            Guid employeeId,
            DateTimeOffset from,
            DateTimeOffset to,
            CancellationToken ct = default)
            => await _context.AppUsageSessions
                .Where(s => s.EmployeeId == employeeId
                         && s.StartedAt >= from
                         && s.StartedAt < to)
                .OrderByDescending(s => s.StartedAt)
                .ToListAsync(ct);

        public async Task<IReadOnlyList<AppUsageSession>> GetByEmployeesAndRangeAsync(
            IReadOnlyCollection<Guid> employeeIds,
            DateTimeOffset from,
            DateTimeOffset to,
            CancellationToken ct = default)
        {
            if (employeeIds.Count == 0)
                return Array.Empty<AppUsageSession>();

            return await _context.AppUsageSessions
                .Where(s => employeeIds.Contains(s.EmployeeId)
                         && s.StartedAt >= from
                         && s.StartedAt < to)
                .ToListAsync(ct);
        }
    }
}
