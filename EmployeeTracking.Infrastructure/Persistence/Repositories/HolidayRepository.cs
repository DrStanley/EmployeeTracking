using EmployeeTracking.Domain.Entities;
using EmployeeTracking.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EmployeeTracking.Infrastructure.Persistence.Repositories
{
    public class HolidayRepository : IHolidayRepository
    {
        private readonly AppDbContext _context;

        public HolidayRepository(AppDbContext context) => _context = context;

        public async Task<Holiday?> GetByIdAsync(
            Guid id, CancellationToken ct = default)
            => await _context.Holidays
                .FirstOrDefaultAsync(h => h.Id == id, ct);

        public async Task<IReadOnlyList<Holiday>> GetAllAsync(
            CancellationToken ct = default)
            => await _context.Holidays
                .Where(h => h.IsActive)
                .OrderBy(h => h.Date)
                .ToListAsync(ct);

        public async Task<IReadOnlyList<Holiday>> GetByYearAsync(
            int year, CancellationToken ct = default)
            => await _context.Holidays
                .Where(h => h.IsActive &&
                    (h.Date.Year == year || h.IsRecurringAnnually))
                .OrderBy(h => h.Date)
                .ToListAsync(ct);

        public async Task AddAsync(Holiday holiday, CancellationToken ct = default)
            => await _context.Holidays.AddAsync(holiday, ct);

        public void Update(Holiday holiday)
            => _context.Holidays.Update(holiday);

        public void Delete(Holiday holiday)
            => _context.Holidays.Remove(holiday);
    }
}
