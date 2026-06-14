using EmployeeTracking.Domain.Interfaces;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmployeeTracking.Domain.Common
{
    public abstract class AuditableEntity
    {
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }

        private readonly List<IDomainEvent> _domainEvents = new();

        [NotMapped]
        public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

        protected void AddDomainEvent(IDomainEvent domainEvent)
            => _domainEvents.Add(domainEvent);

        public void ClearDomainEvents()
            => _domainEvents.Clear();
    }
}
