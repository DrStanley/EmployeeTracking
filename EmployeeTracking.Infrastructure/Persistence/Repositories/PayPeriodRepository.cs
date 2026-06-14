using EmployeeTracking.Domain.Entities;
using EmployeeTracking.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EmployeeTracking.Infrastructure.Persistence.Repositories
{
    public class PayPeriodRepository : IPayPeriodRepository
    {
        private readonly AppDbContext _context;

        public PayPeriodRepository(AppDbContext context) => _context = context;

        public async Task<PayPeriod?> GetByIdAsync(Guid id, CancellationToken ct = default)
            => await _context.PayPeriods.FirstOrDefaultAsync(p => p.Id == id, ct);

        public async Task<PayPeriod?> GetCurrentAsync(CancellationToken ct = default)
        {
            var today = DateOnly.FromDateTime(DateTime.Now);
            return await _context.PayPeriods
                .FirstOrDefaultAsync(
                    p => p.StartDate <= today && p.EndDate >= today, ct);
        }

        public async Task<IReadOnlyList<PayPeriod>> GetAllAsync(CancellationToken ct = default)
            => await _context.PayPeriods
                .OrderByDescending(p => p.StartDate)
                .ToListAsync(ct);

        public async Task AddAsync(PayPeriod payPeriod, CancellationToken ct = default)
            => await _context.PayPeriods.AddAsync(payPeriod, ct);

        public void Update(PayPeriod payPeriod)
            => _context.PayPeriods.Update(payPeriod);
    }
}
