using EmployeeTracking.Domain.Common;

namespace EmployeeTracking.Domain.Entities
{
    public class ScreenReview : AuditableEntity
    {
        public Guid Id { get; private set; }
        public Guid EmployeeId { get; private set; }
        public string EmployeeName { get; private set; } = string.Empty;
        public DateTimeOffset StartedAt { get; private set; }
        public DateTimeOffset EndedAt { get; private set; }
        public int FrameCount { get; private set; }
        public string Findings { get; private set; } = string.Empty;

        private ScreenReview() { }

        public static ScreenReview Record(
            Guid employeeId,
            string employeeName,
            DateTimeOffset startedAt,
            DateTimeOffset endedAt,
            int frameCount,
            IReadOnlyList<string> findings)
        {
            if (employeeId == Guid.Empty)
                throw new DomainException("Employee is required.");
            if (frameCount < 0)
                throw new DomainException("Frame count cannot be negative.");
            if (endedAt < startedAt)
                throw new DomainException("Clock-out is before clock-in.");
            if (findings == null || findings.Count == 0)
                throw new DomainException("A screen review needs at least one finding.");

            var lines = findings
                .Select(line => line?.Trim() ?? string.Empty)
                .Where(line => line.Length > 0)
                .ToArray();
            if (lines.Length == 0)
                throw new DomainException("A screen review needs at least one finding.");

            return new ScreenReview
            {
                Id = Guid.NewGuid(),
                EmployeeId = employeeId,
                EmployeeName = string.IsNullOrWhiteSpace(employeeName) ? "Employee" : employeeName.Trim(),
                StartedAt = startedAt,
                EndedAt = endedAt,
                FrameCount = frameCount,
                Findings = string.Join('\n', lines)
            };
        }

        public string[] FindingLines() =>
            Findings.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }
}
