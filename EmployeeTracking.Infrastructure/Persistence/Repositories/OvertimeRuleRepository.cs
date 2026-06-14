using EmployeeTracking.Domain.Entities;
using EmployeeTracking.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EmployeeTracking.Infrastructure.Persistence.Repositories
{
    public class OvertimeRuleRepository : IOvertimeRuleRepository
    {
        private readonly AppDbContext _context;

        public OvertimeRuleRepository(AppDbContext context) => _context = context;

        public async Task<OvertimeRule?> GetByIdAsync(
            Guid id, CancellationToken ct = default)
            => await _context.OvertimeRules
                .Include(o => o.AttendancePolicy)
                .FirstOrDefaultAsync(o => o.Id == id, ct);

        public async Task<IReadOnlyList<OvertimeRule>> GetAllAsync(
            CancellationToken ct = default)
            => await _context.OvertimeRules
                .Include(o => o.AttendancePolicy)
                .ToListAsync(ct);

        public async Task<OvertimeRule?> GetByPolicyAsync(
            Guid policyId, CancellationToken ct = default)
            => await _context.OvertimeRules
                .Include(o => o.AttendancePolicy)
                .FirstOrDefaultAsync(o => o.AttendancePolicyId == policyId, ct);

        public async Task AddAsync(OvertimeRule rule, CancellationToken ct = default)
            => await _context.OvertimeRules.AddAsync(rule, ct);

        public void Update(OvertimeRule rule)
            => _context.OvertimeRules.Update(rule);

        public void Delete(OvertimeRule rule)
            => _context.OvertimeRules.Remove(rule);
    }
}
