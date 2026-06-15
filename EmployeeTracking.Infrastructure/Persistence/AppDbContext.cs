using EmployeeTracking.Domain.Common;
using EmployeeTracking.Domain.Entities;
using EmployeeTracking.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace EmployeeTracking.Infrastructure.Persistence
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Employee> Employees => Set<Employee>();
        public DbSet<Department> Departments => Set<Department>();
        public DbSet<Shift> Shifts => Set<Shift>();
        public DbSet<TimeEntry> TimeEntries => Set<TimeEntry>();
        public DbSet<BreakEntry> BreakEntries => Set<BreakEntry>();
        public DbSet<Timesheet> Timesheets => Set<Timesheet>();
        public DbSet<TimesheetLine> TimesheetLines => Set<TimesheetLine>();
        public DbSet<ApprovalAction> ApprovalActions => Set<ApprovalAction>();
        public DbSet<PTORequest> PTORequests => Set<PTORequest>();
        public DbSet<PTOBalance> PTOBalances => Set<PTOBalance>();
        public DbSet<OvertimeRule> OvertimeRules => Set<OvertimeRule>();
        public DbSet<AttendancePolicy> AttendancePolicies => Set<AttendancePolicy>();
        public DbSet<PayPeriod> PayPeriods => Set<PayPeriod>();
        public DbSet<PayrollReport> PayrollReports => Set<PayrollReport>();
        public DbSet<Notification> Notifications => Set<Notification>();
        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
        public DbSet<Holiday> Holidays => Set<Holiday>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }

        public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
        {
            foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
            {
                if (entry.State == EntityState.Added)
                    entry.Entity.CreatedAt = DateTimeOffset.UtcNow;
                if (entry.State is EntityState.Added or EntityState.Modified)
                    entry.Entity.UpdatedAt = DateTimeOffset.UtcNow;
            }
            return await base.SaveChangesAsync(ct);
        }
    }
}
