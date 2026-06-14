using EmployeeTracking.Domain.Common;

namespace EmployeeTracking.Domain.Entities
{
    public class Holiday : AuditableEntity
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; } = string.Empty;
        public DateOnly Date { get; private set; }
        public bool IsRecurringAnnually { get; private set; }
        public bool IsActive { get; private set; } = true;

        private Holiday() { }

        public static Holiday Create(
            string name, DateOnly date, bool isRecurringAnnually) =>
            new()
            {
                Id = Guid.NewGuid(),
                Name = name,
                Date = date,
                IsRecurringAnnually = isRecurringAnnually
            };

        public void Update(string name, DateOnly date, bool isRecurringAnnually)
        {
            Name = name;
            Date = date;
            IsRecurringAnnually = isRecurringAnnually;
        }

        public void Deactivate() => IsActive = false;
    }
}
