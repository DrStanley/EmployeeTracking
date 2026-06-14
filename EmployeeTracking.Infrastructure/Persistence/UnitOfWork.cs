using EmployeeTracking.Application.Interfaces;
using EmployeeTracking.Domain.Interfaces.Repositories;
using EmployeeTracking.Infrastructure.Persistence.Repositories;

namespace EmployeeTracking.Infrastructure.Persistence
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;

        public IEmployeeRepository Employees { get; }
        public ITimeEntryRepository TimeEntries { get; }
        public ITimesheetRepository Timesheets { get; }
        public IPTORepository PTORequests { get; }
        public IPTOBalanceRepository PTOBalances { get; }
        public IPayPeriodRepository PayPeriods { get; }
        public IPayrollReportRepository PayrollReports { get; }
        public INotificationRepository Notifications { get; }

        public UnitOfWork(AppDbContext context)
        {
            _context = context;
            Employees = new EmployeeRepository(context);
            TimeEntries = new TimeEntryRepository(context);
            Timesheets = new TimesheetRepository(context);
            PTORequests = new PTORepository(context);
            PTOBalances = new PTOBalanceRepository(context);
            PayPeriods = new PayPeriodRepository(context);
            PayrollReports = new PayrollReportRepository(context);
            Notifications = new NotificationRepository(context);
        }

        public async Task<int> SaveChangesAsync(CancellationToken ct = default)
            => await _context.SaveChangesAsync(ct);

        public void Dispose() => _context.Dispose();
    }
}
