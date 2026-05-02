using EmployeeTracking.Domain.Interfaces;

namespace EmployeeTracking.Domain.DomainEvents
{
    public record TimesheetApprovedEvent(
    Guid TimesheetId,
    Guid EmployeeId,
    Guid ReviewerId) : IDomainEvent;
}
