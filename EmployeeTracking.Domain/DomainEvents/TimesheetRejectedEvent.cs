using EmployeeTracking.Domain.Common;

namespace EmployeeTracking.Domain.DomainEvents
{
    public record TimesheetRejectedEvent(
    Guid TimesheetId,
    Guid EmployeeId,
    Guid ReviewerId,
    string Reason) : IDomainEvent;
}
