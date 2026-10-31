using EmployeeTracking.Domain.Common;

namespace EmployeeTracking.Domain.Entities
{
    public class ScreenCaptureFrame : AuditableEntity
    {
        public Guid Id { get; private set; }
        public Guid EmployeeId { get; private set; }
        public DateTimeOffset CapturedAt { get; private set; }
        public string StorageKey { get; private set; } = string.Empty;

        private ScreenCaptureFrame() { }

        public static ScreenCaptureFrame Record(Guid employeeId, DateTimeOffset capturedAt, string storageKey)
        {
            if (employeeId == Guid.Empty)
                throw new DomainException("Employee is required.");
            if (string.IsNullOrWhiteSpace(storageKey))
                throw new DomainException("A storage key is required.");

            return new ScreenCaptureFrame
            {
                Id = Guid.NewGuid(),
                EmployeeId = employeeId,
                CapturedAt = capturedAt,
                StorageKey = storageKey.Trim()
            };
        }
    }
}
