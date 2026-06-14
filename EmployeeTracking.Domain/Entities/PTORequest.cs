using EmployeeTracking.Domain.Common;
using EmployeeTracking.Domain.Enums;
using EmployeeTracking.Domain.ValueObjects;

namespace EmployeeTracking.Domain.Entities
{
    public class PTORequest : AuditableEntity
    {
        public Guid Id { get; private set; }
        public Guid EmployeeId { get; private set; }
        public DateOnly StartDate { get; private set; }
        public DateOnly EndDate { get; private set; }
        public decimal HoursRequested { get; private set; }
        public PTORequestStatus Status { get; private set; }
        public string? Notes { get; private set; }
        public Guid? ReviewedBy { get; private set; }
        public string? ReviewerNotes { get; private set; }
        public DateTimeOffset? ReviewedAt { get; private set; }

        public Employee Employee { get; private set; } = null!;

        private PTORequest() { }

        public static PTORequest Create(
            Guid employeeId,
            DateRange range,
            decimal hoursRequested,
            string? notes = null) =>
            new()
            {
                Id = Guid.NewGuid(),
                EmployeeId = employeeId,
                StartDate = range.Start,
                EndDate = range.End,
                HoursRequested = hoursRequested,
                Notes = notes,
                Status = PTORequestStatus.Pending
            };

        public void Approve(Guid reviewerId, string? notes = null)
        {
            if (Status != PTORequestStatus.Pending)
                throw new DomainException("Only pending requests can be approved.");
            Status = PTORequestStatus.Approved;
            ReviewedBy = reviewerId;
            ReviewerNotes = notes;
            ReviewedAt = DateTimeOffset.Now;
        }

        public void Reject(Guid reviewerId, string reason)
        {
            if (Status != PTORequestStatus.Pending)
                throw new DomainException("Only pending requests can be rejected.");
            Status = PTORequestStatus.Rejected;
            ReviewedBy = reviewerId;
            ReviewerNotes = reason;
            ReviewedAt = DateTimeOffset.Now;
        }

        public void Cancel()
        {
            if (Status == PTORequestStatus.Approved)
                throw new DomainException("Contact an admin to cancel an approved request.");
            Status = PTORequestStatus.Cancelled;
        }

        public DateRange ToDateRange() => new(StartDate, EndDate);
    }

}
