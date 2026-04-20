using EmployeeTracking.Domain.Common;

namespace EmployeeTracking.Domain.DomainEvents
{
    public record TimesheetApprovedEvent(
    Guid TimesheetId,
    Guid EmployeeId,
    Guid ReviewerId) : IDomainEvent;
}
