using EmployeeTracking.Domain.Common;

namespace EmployeeTracking.Domain.DomainEvents
{
    public record TimesheetSubmittedEvent(Guid TimesheetId, Guid EmployeeId) : IDomainEvent;
}
