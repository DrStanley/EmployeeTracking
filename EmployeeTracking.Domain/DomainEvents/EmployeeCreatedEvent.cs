using EmployeeTracking.Domain.Common;

namespace EmployeeTracking.Domain.DomainEvents
{
    public record EmployeeCreatedEvent(Guid EmployeeId) : IDomainEvent;
}
