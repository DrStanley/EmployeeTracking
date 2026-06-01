using EmployeeTracking.Domain.Entities;
using EmployeeTracking.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EmployeeTracking.Infrastructure.Persistence.Repositories
{
    public class PayrollReportRepository : IPayrollReportRepository
    {
        private readonly AppDbContext _context;

        public PayrollReportRepository(AppDbContext context) => _context = context;

        public async Task<IReadOnlyList<PayrollReport>> GetByPayPeriodAsync(
            Guid payPeriodId, CancellationToken ct = default)
            => await _context.PayrollReports
                .Include(r => r.Employee)
                    .ThenInclude(e => e.Department)
                .Include(r => r.PayPeriod)
                .Where(r => r.PayPeriodId == payPeriodId)
                .OrderBy(r => r.Employee.LastName)
                .ToListAsync(ct);

        public async Task<PayrollReport?> GetByEmployeeAndPeriodAsync(
            Guid employeeId, Guid payPeriodId, CancellationToken ct = default)
            => await _context.PayrollReports
                .FirstOrDefaultAsync(
                    r => r.EmployeeId == employeeId
                      && r.PayPeriodId == payPeriodId, ct);

        public async Task AddAsync(PayrollReport report, CancellationToken ct = default)
            => await _context.PayrollReports.AddAsync(report, ct);

        public void Update(PayrollReport report)
            => _context.PayrollReports.Update(report);
    }
}
