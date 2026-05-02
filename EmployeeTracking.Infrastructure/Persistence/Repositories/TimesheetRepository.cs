using EmployeeTracking.Domain.Entities;
using EmployeeTracking.Domain.Enums;
using EmployeeTracking.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EmployeeTracking.Infrastructure.Persistence.Repositories
{
    public class TimesheetRepository : ITimesheetRepository
    {
        private readonly AppDbContext _context;

        public TimesheetRepository(AppDbContext context) => _context = context;

        public async Task<Timesheet?> GetByIdAsync(Guid id, CancellationToken ct = default)
            => await _context.Timesheets
                .Include(t => t.Lines)
                .Include(t => t.ApprovalActions)
                .FirstOrDefaultAsync(t => t.Id == id, ct);

        public async Task<Timesheet?> GetByEmployeeAndPeriodAsync(
            Guid employeeId, Guid payPeriodId, CancellationToken ct = default)
            => await _context.Timesheets
                .Include(t => t.Lines)
                .FirstOrDefaultAsync(
                    t => t.EmployeeId == employeeId
                      && t.PayPeriodId == payPeriodId, ct);

        public async Task<IReadOnlyList<Timesheet>> GetByEmployeeAsync(
            Guid employeeId, CancellationToken ct = default)
            => await _context.Timesheets
                .Where(t => t.EmployeeId == employeeId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync(ct);

        public async Task<IReadOnlyList<Timesheet>> GetPendingApprovalsForManagerAsync(
            Guid managerId, CancellationToken ct = default)
            => await _context.Timesheets
                .Include(t => t.Lines)
                .Where(t => t.Status == TimesheetStatus.Submitted
                         && _context.Employees
                                .Any(e => e.Id == t.EmployeeId
                                       && e.ManagerId == managerId))
                .ToListAsync(ct);

        public async Task AddAsync(Timesheet timesheet, CancellationToken ct = default)
            => await _context.Timesheets.AddAsync(timesheet, ct);

        public void Update(Timesheet timesheet)
            => _context.Timesheets.Update(timesheet);
    }
}
