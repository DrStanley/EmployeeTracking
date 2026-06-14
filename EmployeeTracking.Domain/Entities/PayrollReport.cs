using EmployeeTracking.Domain.Common;

namespace EmployeeTracking.Domain.Entities
{
    public class PayrollReport : AuditableEntity
    {
        public Guid Id { get; private set; }
        public Guid PayPeriodId { get; private set; }
        public Guid EmployeeId { get; private set; }
        public decimal RegularHours { get; private set; }
        public decimal OvertimeHours { get; private set; }
        public decimal PTOHours { get; private set; }
        public decimal UnpaidHours { get; private set; }
        public decimal TotalPayableHours { get; private set; }
        public bool HasExceptions { get; private set; }
        public string? ExceptionNotes { get; private set; }
        public DateTimeOffset GeneratedAt { get; private set; }

        public PayPeriod PayPeriod { get; private set; } = null!;
        public Employee Employee { get; private set; } = null!;

        private PayrollReport() { }

        public static PayrollReport Generate(
            Guid payPeriodId,
            Guid employeeId,
            decimal regular,
            decimal overtime,
            decimal pto,
            decimal unpaid,
            string? exceptionNotes = null) =>
            new()
            {
                Id = Guid.NewGuid(),
                PayPeriodId = payPeriodId,
                EmployeeId = employeeId,
                RegularHours = regular,
                OvertimeHours = overtime,
                PTOHours = pto,
                UnpaidHours = unpaid,
                TotalPayableHours = regular + overtime + pto,
                HasExceptions = exceptionNotes != null,
                ExceptionNotes = exceptionNotes,
                GeneratedAt = DateTimeOffset.Now
            };
    }
}
