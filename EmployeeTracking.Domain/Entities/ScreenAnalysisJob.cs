using EmployeeTracking.Domain.Common;

namespace EmployeeTracking.Domain.Entities
{
    public enum ScreenAnalysisStatus
    {
        Pending,
        Running,
        Failed
    }

    public class ScreenAnalysisJob : AuditableEntity
    {
        public Guid Id { get; private set; }
        public Guid EmployeeId { get; private set; }
        public string EmployeeName { get; private set; } = string.Empty;
        public DateTimeOffset StartedAt { get; private set; }
        public DateTimeOffset EndedAt { get; private set; }
        public int FrameCount { get; private set; }
        public ScreenAnalysisStatus Status { get; private set; }
        public int Attempts { get; private set; }
        public string? LastError { get; private set; }

        private ScreenAnalysisJob() { }

        public static ScreenAnalysisJob Schedule(
            Guid employeeId,
            string employeeName,
            DateTimeOffset startedAt,
            DateTimeOffset endedAt,
            int frameCount)
        {
            if (employeeId == Guid.Empty)
                throw new DomainException("Employee is required.");
            if (endedAt < startedAt)
                throw new DomainException("Clock-out is before clock-in.");
            if (frameCount < 0)
                throw new DomainException("Frame count cannot be negative.");

            return new ScreenAnalysisJob
            {
                Id = Guid.NewGuid(),
                EmployeeId = employeeId,
                EmployeeName = string.IsNullOrWhiteSpace(employeeName) ? "Employee" : employeeName.Trim(),
                StartedAt = startedAt,
                EndedAt = endedAt,
                FrameCount = frameCount,
                Status = ScreenAnalysisStatus.Pending
            };
        }

        public void MarkRunning()
        {
            Status = ScreenAnalysisStatus.Running;
            Attempts++;
        }

        public void MarkForRetry(string error)
        {
            LastError = string.IsNullOrWhiteSpace(error) ? "Screen analysis failed." : error.Trim();
            if (LastError.Length > 1000) LastError = LastError[..1000];
            Status = Attempts >= 3 ? ScreenAnalysisStatus.Failed : ScreenAnalysisStatus.Pending;
        }
    }
}
