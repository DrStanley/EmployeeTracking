using EmployeeTracking.Domain.Common;
using EmployeeTracking.Domain.Enums;

namespace EmployeeTracking.Domain.Entities
{
    public class Notification : AuditableEntity
    {
        public Guid Id { get; private set; }
        public Guid EmployeeId { get; private set; }
        public NotificationType Type { get; private set; }
        public string Message { get; private set; } = string.Empty;
        public bool IsRead { get; private set; }
        public DateTimeOffset SentAt { get; private set; }

        public Employee Employee { get; private set; } = null!;

        private Notification() { }

        public static Notification Create(
            Guid employeeId,
            NotificationType type,
            string message) =>
            new()
            {
                Id = Guid.NewGuid(),
                EmployeeId = employeeId,
                Type = type,
                Message = message,
                IsRead = false,
                SentAt = DateTimeOffset.Now
            };

        public void MarkRead() => IsRead = true;
    }

}
