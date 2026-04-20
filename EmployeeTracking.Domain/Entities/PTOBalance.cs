using EmployeeTracking.Domain.Common;

namespace EmployeeTracking.Domain.Entities
{
    public class PTOBalance : AuditableEntity
    {
        public Guid Id { get; private set; }
        public Guid EmployeeId { get; private set; }
        public decimal AvailableHours { get; private set; }
        public decimal UsedHours { get; private set; }
        public decimal AccruedHours { get; private set; }
        public int Year { get; private set; }

        public Employee Employee { get; private set; } = null!;

        private PTOBalance() { }

        public static PTOBalance CreateForYear(Guid employeeId, int year) =>
            new()
            {
                Id = Guid.NewGuid(),
                EmployeeId = employeeId,
                Year = year,
                AvailableHours = 0m,
                UsedHours = 0m,
                AccruedHours = 0m
            };

        public void Accrue(decimal hours)
        {
            if (hours <= 0) throw new DomainException("Accrual hours must be positive.");
            AccruedHours += hours;
            AvailableHours += hours;
        }

        public void Deduct(decimal hours)
        {
            if (hours > AvailableHours)
                throw new DomainException($"Insufficient PTO balance. Available: {AvailableHours}h, Requested: {hours}h.");
            AvailableHours -= hours;
            UsedHours += hours;
        }

        public void Restore(decimal hours)
        {
            AvailableHours += hours;
            UsedHours -= hours;
        }
    }
}
