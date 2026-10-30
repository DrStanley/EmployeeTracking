using EmployeeTracking.Domain.Common;
using EmployeeTracking.Domain.Enums;

namespace EmployeeTracking.Domain.Entities
{
    /// <summary>
    /// A finished stretch of time an employee spent in one application.
    /// </summary>
    public class AppUsageSession : AuditableEntity
    {
        public const int MaxAppNameLength = 100;
        public static readonly TimeSpan MaxDuration = TimeSpan.FromHours(24);

        public Guid Id { get; private set; }
        public Guid EmployeeId { get; private set; }
        public string AppName { get; private set; } = string.Empty;
        public DateTimeOffset StartedAt { get; private set; }
        public DateTimeOffset EndedAt { get; private set; }
        public int DurationSeconds { get; private set; }
        public AppUsageSource Source { get; private set; }

        public decimal DurationHours =>
            Math.Round(DurationSeconds / 3600m, 2, MidpointRounding.AwayFromZero);

        private AppUsageSession() { }

        public static AppUsageSession Record(
            Guid employeeId,
            string appName,
            DateTimeOffset startedAt,
            DateTimeOffset endedAt,
            AppUsageSource source)
        {
            if (employeeId == Guid.Empty)
                throw new DomainException("Employee is required.");

            var name = appName?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(name))
                throw new DomainException("App name is required.");
            if (name.Length > MaxAppNameLength)
                throw new DomainException($"App name cannot exceed {MaxAppNameLength} characters.");

            if (endedAt <= startedAt)
                throw new DomainException("End time must be after start time.");

            var duration = endedAt - startedAt;
            if (duration > MaxDuration)
                throw new DomainException("A single activity session cannot exceed 24 hours.");

            var now = DateTimeOffset.UtcNow;
            if (startedAt > now.AddMinutes(5))
                throw new DomainException("Activity cannot start in the future.");
            if (endedAt > now.AddMinutes(5))
                throw new DomainException("Activity cannot end in the future.");

            var seconds = (int)Math.Round(duration.TotalSeconds, MidpointRounding.AwayFromZero);
            if (seconds <= 0)
                throw new DomainException("End time must be after start time.");

            return new AppUsageSession
            {
                Id = Guid.NewGuid(),
                EmployeeId = employeeId,
                AppName = name,
                StartedAt = startedAt,
                EndedAt = endedAt,
                DurationSeconds = seconds,
                Source = source
            };
        }
    }
}
