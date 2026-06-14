using EmployeeTracking.Domain.Common;

namespace EmployeeTracking.Domain.Entities
{
    public class BreakEntry : AuditableEntity
    {
        public Guid Id { get; private set; }
        public Guid TimeEntryId { get; private set; }
        public Guid EmployeeId { get; private set; }
        public DateTimeOffset BreakStart { get; private set; }
        public DateTimeOffset? BreakEnd { get; private set; }
        public bool IsPaid { get; private set; }

        public TimeEntry TimeEntry { get; private set; } = null!;

        private BreakEntry() { }

        public static BreakEntry Start(Guid timeEntryId, Guid employeeId, bool isPaid) =>
            new()
            {
                Id = Guid.NewGuid(),
                TimeEntryId = timeEntryId,
                EmployeeId = employeeId,
                BreakStart = DateTimeOffset.Now,
                IsPaid = isPaid
            };

        public void End()
        {
            if (BreakEnd.HasValue)
                throw new DomainException("Break has already ended.");
            BreakEnd = DateTimeOffset.Now;
        }

        public decimal DurationHours =>
            BreakEnd.HasValue
                ? (decimal)(BreakEnd.Value - BreakStart).TotalHours
                : 0m;
    }

}
