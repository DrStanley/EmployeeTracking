using EmployeeTracking.Domain.Interfaces;

namespace EmployeeTracking.Domain.DomainEvents
{
    public record EmployeeCreatedEvent(Guid EmployeeId) : IDomainEvent;
}
