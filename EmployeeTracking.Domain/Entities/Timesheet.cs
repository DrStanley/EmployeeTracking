using EmployeeTracking.Domain.Common;
using EmployeeTracking.Domain.Enums;

namespace EmployeeTracking.Domain.Entities
{
    public class Timesheet : AuditableEntity
    {
        public Guid Id { get; private set; }
        public Guid EmployeeId { get; private set; }
        public Guid PayPeriodId { get; private set; }
        public TimesheetStatus Status { get; private set; }
        public decimal TotalRegularHours { get; private set; }
        public decimal TotalOvertimeHours { get; private set; }
        public decimal TotalPTOHours { get; private set; }
        public decimal TotalUnpaidHours { get; private set; }
        public string? RejectionReason { get; private set; }
        public DateTimeOffset? SubmittedAt { get; private set; }
        public DateTimeOffset? ApprovedAt { get; private set; }

        private readonly List<TimesheetLine> _lines = new();
        public IReadOnlyList<TimesheetLine> Lines => _lines.AsReadOnly();

        private readonly List<ApprovalAction> _approvalActions = new();
        public IReadOnlyList<ApprovalAction> ApprovalActions => _approvalActions.AsReadOnly();

        private Timesheet() { }

        public static Timesheet CreateForPeriod(Guid employeeId, Guid payPeriodId) =>
            new()
            {
                Id = Guid.NewGuid(),
                EmployeeId = employeeId,
                PayPeriodId = payPeriodId,
                Status = TimesheetStatus.Draft
            };

        public void Submit()
        {
            if (Status != TimesheetStatus.Draft && Status != TimesheetStatus.Rejected)
                throw new DomainException($"Cannot submit a timesheet with status '{Status}'.");
            Status = TimesheetStatus.Submitted;
            SubmittedAt = DateTimeOffset.Now;
            AddDomainEvent(new DomainEvents.TimesheetSubmittedEvent(Id, EmployeeId));
        }

        public void Approve(Guid reviewerId, string? notes = null)
        {
            if (Status != TimesheetStatus.Submitted)
                throw new DomainException("Only submitted timesheets can be approved.");
            Status = TimesheetStatus.Approved;
            ApprovedAt = DateTimeOffset.Now;
            _approvalActions.Add(ApprovalAction.Record(Id, reviewerId, ApprovalDecision.Approved, notes));
            AddDomainEvent(new DomainEvents.TimesheetApprovedEvent(Id, EmployeeId, reviewerId));
        }

        public void Reject(Guid reviewerId, string reason)
        {
            if (Status != TimesheetStatus.Submitted)
                throw new DomainException("Only submitted timesheets can be rejected.");
            if (string.IsNullOrWhiteSpace(reason))
                throw new DomainException("A rejection reason is required.");
            Status = TimesheetStatus.Rejected;
            RejectionReason = reason;
            _approvalActions.Add(ApprovalAction.Record(Id, reviewerId, ApprovalDecision.Rejected, reason));
            AddDomainEvent(new DomainEvents.TimesheetRejectedEvent(Id, EmployeeId, reviewerId, reason));
        }

        public void Lock()
        {
            if (Status != TimesheetStatus.Approved)
                throw new DomainException("Only approved timesheets can be locked.");
            Status = TimesheetStatus.Locked;
        }

        public void CalculateTotals(
            decimal regular, decimal overtime, decimal pto, decimal unpaid)
        {
            TotalRegularHours = regular;
            TotalOvertimeHours = overtime;
            TotalPTOHours = pto;
            TotalUnpaidHours = unpaid;
        }

        public void AddLine(TimesheetLine line) => _lines.Add(line);
    }
}
