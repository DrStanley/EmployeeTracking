using EmployeeTracking.Domain.Common;

namespace EmployeeTracking.Domain.Entities
{
    public class AuditLog : AuditableEntity
    {
        public Guid Id { get; private set; }
        public string EntityName { get; private set; } = string.Empty;
        public string EntityId { get; private set; } = string.Empty;
        public string Action { get; private set; } = string.Empty;
        public string? OldValues { get; private set; }
        public string? NewValues { get; private set; }
        public Guid? PerformedBy { get; private set; }
        public DateTimeOffset Timestamp { get; private set; }

        private AuditLog() { }

        public static AuditLog Record(
            string entityName,
            string entityId,
            string action,
            Guid? performedBy,
            string? oldValues = null,
            string? newValues = null) =>
            new()
            {
                Id = Guid.NewGuid(),
                EntityName = entityName,
                EntityId = entityId,
                Action = action,
                PerformedBy = performedBy,
                OldValues = oldValues,
                NewValues = newValues,
                Timestamp = DateTimeOffset.Now
            };
    }

}
