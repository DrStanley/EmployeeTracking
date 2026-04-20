using EmployeeTracking.Domain.Common;
using EmployeeTracking.Domain.Enums;
using EmployeeTracking.Domain.ValueObjects;

namespace EmployeeTracking.Domain.Entities
{
    public class TimeEntry : AuditableEntity
    {
        public Guid Id { get; private set; }
        public Guid EmployeeId { get; private set; }
        public TimeEntryType EntryType { get; private set; }
        public TimeEntrySource Source { get; private set; }
        public DateTimeOffset Timestamp { get; private set; }
        public LocationMetadata? Location { get; private set; }
        public string? Notes { get; private set; }
        public bool IsDeleted { get; private set; }
        public Guid? CorrectedFromId { get; private set; }

        private readonly List<BreakEntry> _breaks = new();
        public IReadOnlyList<BreakEntry> Breaks => _breaks.AsReadOnly();

        private TimeEntry() { }

        public static TimeEntry Create(
            Guid employeeId,
            TimeEntryType type,
            TimeEntrySource source,
            DateTimeOffset timestamp,
            LocationMetadata? location = null,
            string? notes = null) =>
            new()
            {
                Id = Guid.NewGuid(),
                EmployeeId = employeeId,
                EntryType = type,
                Source = source,
                Timestamp = timestamp,
                Location = location,
                Notes = notes
            };

        public TimeEntry CreateCorrection(
            TimeEntryType newType,
            DateTimeOffset newTimestamp,
            string reason)
        {
            var correction = Create(
                EmployeeId, newType,
                TimeEntrySource.ManagerEntry,
                newTimestamp, notes: reason);
            correction.CorrectedFromId = Id;
            IsDeleted = true;
            return correction;
        }
    }

}
