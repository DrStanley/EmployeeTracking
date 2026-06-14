using EmployeeTracking.Domain.Entities;
using EmployeeTracking.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EmployeeTracking.Infrastructure.Persistence.Repositories
{
    public class ShiftRepository : IShiftRepository
    {
        private readonly AppDbContext _context;

        public ShiftRepository(AppDbContext context) => _context = context;

        public async Task<Shift?> GetByIdAsync(
            Guid id, CancellationToken ct = default)
            => await _context.Shifts
                .FirstOrDefaultAsync(s => s.Id == id, ct);

        public async Task<IReadOnlyList<Shift>> GetAllAsync(
            CancellationToken ct = default)
            => await _context.Shifts
                .Where(s => s.IsActive)
                .OrderBy(s => s.PlannedStart)
                .ToListAsync(ct);

        public async Task AddAsync(Shift shift, CancellationToken ct = default)
            => await _context.Shifts.AddAsync(shift, ct);

        public void Update(Shift shift)
            => _context.Shifts.Update(shift);

        public void Delete(Shift shift)
            => _context.Shifts.Remove(shift);
    }
}
