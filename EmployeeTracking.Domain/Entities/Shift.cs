using EmployeeTracking.Domain.Common;

namespace EmployeeTracking.Domain.Entities
{
    public class Shift : AuditableEntity
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; } = string.Empty;
        public TimeOnly PlannedStart { get; private set; }
        public TimeOnly PlannedEnd { get; private set; }
        public int GracePeriodMinutes { get; private set; }
        public bool IsActive { get; private set; } = true;

        private Shift() { }

        public static Shift Create(string name, TimeOnly start, TimeOnly end, int gracePeriodMinutes = 5) =>
            new()
            {
                Id = Guid.NewGuid(),
                Name = name,
                PlannedStart = start,
                PlannedEnd = end,
                GracePeriodMinutes = gracePeriodMinutes
            };
    }

}
