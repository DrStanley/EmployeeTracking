using EmployeeTracking.Domain.Common;
using EmployeeTracking.Domain.Enums;

namespace EmployeeTracking.Domain.Entities
{
    public class ApprovalAction : AuditableEntity
    {
        public Guid Id { get; private set; }
        public Guid TimesheetId { get; private set; }
        public Guid ReviewerId { get; private set; }
        public ApprovalDecision Decision { get; private set; }
        public string? Notes { get; private set; }
        public DateTimeOffset DecidedAt { get; private set; }

        private ApprovalAction() { }

        public static ApprovalAction Record(
            Guid timesheetId,
            Guid reviewerId,
            ApprovalDecision decision,
            string? notes = null) =>
            new()
            {
                Id = Guid.NewGuid(),
                TimesheetId = timesheetId,
                ReviewerId = reviewerId,
                Decision = decision,
                Notes = notes,
                DecidedAt = DateTimeOffset.UtcNow
            };
    }
}
