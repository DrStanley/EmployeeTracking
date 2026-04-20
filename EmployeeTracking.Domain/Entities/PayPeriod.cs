using EmployeeTracking.Domain.Common;
using EmployeeTracking.Domain.ValueObjects;

namespace EmployeeTracking.Domain.Entities
{
    public class PayPeriod : AuditableEntity
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; } = string.Empty;
        public DateOnly StartDate { get; private set; }
        public DateOnly EndDate { get; private set; }
        public bool IsLocked { get; private set; }

        private PayPeriod() { }

        public static PayPeriod Create(string name, DateOnly start, DateOnly end) =>
            new()
            {
                Id = Guid.NewGuid(),
                Name = name,
                StartDate = start,
                EndDate = end
            };

        public void Lock() => IsLocked = true;

        public DateRange ToDateRange() => new(StartDate, EndDate);
    }
}
