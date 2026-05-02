using EmployeeTracking.Domain.Entities;
using EmployeeTracking.Domain.Enums;
using EmployeeTracking.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EmployeeTracking.Infrastructure.Persistence.Repositories
{
    public class TimeEntryRepository : ITimeEntryRepository
    {
        private readonly AppDbContext _context;

        public TimeEntryRepository(AppDbContext context) => _context = context;

        public async Task<TimeEntry?> GetByIdAsync(Guid id, CancellationToken ct = default)
            => await _context.TimeEntries
                .Include(t => t.Breaks)
                .FirstOrDefaultAsync(t => t.Id == id, ct);

        public async Task<IReadOnlyList<TimeEntry>> GetByEmployeeAsync(
            Guid employeeId, CancellationToken ct = default)
            => await _context.TimeEntries
                .Where(t => t.EmployeeId == employeeId)
                .OrderByDescending(t => t.Timestamp)
                .ToListAsync(ct);

        public async Task<IReadOnlyList<TimeEntry>> GetByEmployeeAndDateRangeAsync(
            Guid employeeId, DateTimeOffset from, DateTimeOffset to, CancellationToken ct = default)
            => await _context.TimeEntries
                .Where(t => t.EmployeeId == employeeId
                         && t.Timestamp >= from
                         && t.Timestamp <= to)
                .OrderBy(t => t.Timestamp)
                .ToListAsync(ct);

        public async Task<bool> HasOpenClockInAsync(
            Guid employeeId, CancellationToken ct = default)
        {
            var lastEntry = await _context.TimeEntries
                .Where(t => t.EmployeeId == employeeId
                         && (t.EntryType == TimeEntryType.ClockIn
                          || t.EntryType == TimeEntryType.ClockOut))
                .OrderByDescending(t => t.Timestamp)
                .FirstOrDefaultAsync(ct);

            return lastEntry?.EntryType == TimeEntryType.ClockIn;
        }

        public async Task<TimeEntry?> GetLatestOpenClockInAsync(
            Guid employeeId, CancellationToken ct = default)
            => await _context.TimeEntries
                .Where(t => t.EmployeeId == employeeId
                         && t.EntryType == TimeEntryType.ClockIn)
                .OrderByDescending(t => t.Timestamp)
                .FirstOrDefaultAsync(ct);

        public async Task AddAsync(TimeEntry entry, CancellationToken ct = default)
            => await _context.TimeEntries.AddAsync(entry, ct);

        public void Update(TimeEntry entry)
            => _context.TimeEntries.Update(entry);
    }
}
