using EmployeeTracking.Domain.Common;

namespace EmployeeTracking.Domain.Entities
{
    public class TimesheetLine : AuditableEntity
    {
        public Guid Id { get; private set; }
        public Guid TimesheetId { get; private set; }
        public DateOnly WorkDate { get; private set; }
        public decimal RegularHours { get; private set; }
        public decimal OvertimeHours { get; private set; }
        public decimal BreakHours { get; private set; }
        public decimal PTOHours { get; private set; }
        public string? Notes { get; private set; }

        private TimesheetLine() { }

        public static TimesheetLine Create(
            Guid timesheetId,
            DateOnly workDate,
            decimal regularHours,
            decimal overtimeHours,
            decimal breakHours = 0m,
            decimal ptoHours = 0m,
            string? notes = null) =>
            new()
            {
                Id = Guid.NewGuid(),
                TimesheetId = timesheetId,
                WorkDate = workDate,
                RegularHours = regularHours,
                OvertimeHours = overtimeHours,
                BreakHours = breakHours,
                PTOHours = ptoHours,
                Notes = notes
            };

        public decimal TotalHours => RegularHours + OvertimeHours + PTOHours;
    }
}
