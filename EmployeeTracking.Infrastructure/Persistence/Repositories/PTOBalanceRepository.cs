using EmployeeTracking.Domain.Entities;
using EmployeeTracking.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EmployeeTracking.Infrastructure.Persistence.Repositories
{
    public class PTOBalanceRepository : IPTOBalanceRepository
    {
        private readonly AppDbContext _context;

        public PTOBalanceRepository(AppDbContext context) => _context = context;

        public async Task<PTOBalance?> GetByEmployeeAndYearAsync(
            Guid employeeId, int year, CancellationToken ct = default)
            => await _context.PTOBalances
                .FirstOrDefaultAsync(
                    b => b.EmployeeId == employeeId && b.Year == year, ct);

        public async Task AddAsync(PTOBalance balance, CancellationToken ct = default)
            => await _context.PTOBalances.AddAsync(balance, ct);

        public void Update(PTOBalance balance)
            => _context.PTOBalances.Update(balance);
    }
}
