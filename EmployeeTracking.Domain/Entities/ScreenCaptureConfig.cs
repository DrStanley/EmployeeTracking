using EmployeeTracking.Domain.Common;

namespace EmployeeTracking.Domain.Entities
{
    public class ScreenCaptureConfig : AuditableEntity
    {
        public static readonly Guid DefaultId = Guid.Parse("b7c1a2e4-6d5f-4a18-9c33-1e0f8a7b2d44");

        public Guid Id { get; private set; }
        public bool Enabled { get; private set; }
        public int IntervalMinutes { get; private set; }

        private ScreenCaptureConfig() { }

        public static ScreenCaptureConfig CreateDefault() => new()
        {
            Id = DefaultId,
            Enabled = true,
            IntervalMinutes = 5
        };

        public void Update(bool enabled, int intervalMinutes)
        {
            if (intervalMinutes < 1 || intervalMinutes > 60)
                throw new DomainException("Capture interval must be between 1 and 60 minutes.");

            Enabled = enabled;
            IntervalMinutes = intervalMinutes;
        }
    }
}
