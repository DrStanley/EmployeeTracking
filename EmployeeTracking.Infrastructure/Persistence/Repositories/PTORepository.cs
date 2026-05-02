using EmployeeTracking.Domain.Entities;
using EmployeeTracking.Domain.Enums;
using EmployeeTracking.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EmployeeTracking.Infrastructure.Persistence.Repositories
{
    public class PTORepository : IPTORepository
    {
        private readonly AppDbContext _context;

        public PTORepository(AppDbContext context) => _context = context;

        public async Task<PTORequest?> GetByIdAsync(Guid id, CancellationToken ct = default)
            => await _context.PTORequests
                .Include(p => p.Employee)
                .FirstOrDefaultAsync(p => p.Id == id, ct);

        public async Task<IReadOnlyList<PTORequest>> GetByEmployeeAsync(
            Guid employeeId, CancellationToken ct = default)
            => await _context.PTORequests
                .Where(p => p.EmployeeId == employeeId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync(ct);

        public async Task<IReadOnlyList<PTORequest>> GetPendingByManagerAsync(
            Guid managerId, CancellationToken ct = default)
            => await _context.PTORequests
                .Include(p => p.Employee)
                .Where(p => p.Status == PTORequestStatus.Pending
                         && _context.Employees
                                .Any(e => e.Id == p.EmployeeId
                                       && e.ManagerId == managerId))
                .ToListAsync(ct);

        public async Task<bool> HasOverlappingRequestAsync(
            Guid employeeId, DateOnly start, DateOnly end, CancellationToken ct = default)
            => await _context.PTORequests
                .AnyAsync(p => p.EmployeeId == employeeId
                            && p.Status != PTORequestStatus.Cancelled
                            && p.Status != PTORequestStatus.Rejected
                            && p.StartDate <= end
                            && p.EndDate >= start, ct);

        public async Task AddAsync(PTORequest request, CancellationToken ct = default)
            => await _context.PTORequests.AddAsync(request, ct);

        public void Update(PTORequest request)
            => _context.PTORequests.Update(request);
    }
}
