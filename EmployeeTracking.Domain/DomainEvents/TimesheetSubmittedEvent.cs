using EmployeeTracking.Domain.Interfaces;

namespace EmployeeTracking.Domain.DomainEvents
{
    public record TimesheetSubmittedEvent(Guid TimesheetId, Guid EmployeeId) : IDomainEvent;
}
