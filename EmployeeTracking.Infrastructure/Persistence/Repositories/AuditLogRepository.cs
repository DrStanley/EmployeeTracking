using EmployeeTracking.Domain.Entities;
using EmployeeTracking.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EmployeeTracking.Infrastructure.Persistence.Repositories
{
    public class AuditLogRepository : IAuditLogRepository
    {
        private readonly AppDbContext _context;

        public AuditLogRepository(AppDbContext context) => _context = context;

        public async Task AddAsync(AuditLog log, CancellationToken ct = default)
            => await _context.AuditLogs.AddAsync(log, ct);

        public async Task<IReadOnlyList<AuditLog>> GetRecentAsync(int take, CancellationToken ct = default)
            => await _context.AuditLogs
                .OrderByDescending(log => log.Timestamp)
                .Take(take)
                .ToListAsync(ct);
    }
}
