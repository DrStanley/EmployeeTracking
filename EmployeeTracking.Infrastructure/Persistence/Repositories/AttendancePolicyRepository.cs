using EmployeeTracking.Domain.Entities;
using EmployeeTracking.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EmployeeTracking.Infrastructure.Persistence.Repositories
{
    public class AttendancePolicyRepository : IAttendancePolicyRepository
    {
        private readonly AppDbContext _context;

        public AttendancePolicyRepository(AppDbContext context) => _context = context;

        public async Task<AttendancePolicy?> GetByIdAsync(
            Guid id, CancellationToken ct = default)
            => await _context.AttendancePolicies
                .FirstOrDefaultAsync(p => p.Id == id, ct);

        public async Task<IReadOnlyList<AttendancePolicy>> GetAllAsync(
            CancellationToken ct = default)
            => await _context.AttendancePolicies
                .OrderBy(p => p.Name)
                .ToListAsync(ct);

        public async Task AddAsync(
            AttendancePolicy policy, CancellationToken ct = default)
            => await _context.AttendancePolicies.AddAsync(policy, ct);

        public void Update(AttendancePolicy policy)
            => _context.AttendancePolicies.Update(policy);

        public void Delete(AttendancePolicy policy)
            => _context.AttendancePolicies.Remove(policy);
    }
}
