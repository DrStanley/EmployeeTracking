using EmployeeTracking.Domain.Common;

namespace EmployeeTracking.Domain.Entities
{
    public class OvertimeRule : AuditableEntity
    {
        public Guid Id { get; private set; }
        public Guid AttendancePolicyId { get; private set; }
        public decimal DailyThresholdHours { get; private set; }
        public decimal WeeklyThresholdHours { get; private set; }
        public decimal PremiumMultiplier { get; private set; }
        public bool HasDoubleTime { get; private set; }
        public decimal DoubleTimeDailyThreshold { get; private set; }

        public AttendancePolicy AttendancePolicy { get; private set; } = null!;

        private OvertimeRule() { }

        public static OvertimeRule Standard(Guid policyId) =>
            new()
            {
                Id = Guid.NewGuid(),
                AttendancePolicyId = policyId,
                DailyThresholdHours = 8m,
                WeeklyThresholdHours = 40m,
                PremiumMultiplier = 1.5m,
                HasDoubleTime = false,
                DoubleTimeDailyThreshold = 12m
            };
        public void Update(
    decimal dailyThreshold,
    decimal weeklyThreshold,
    decimal premiumMultiplier,
    bool hasDoubleTime,
    decimal doubleTimeDailyThreshold)
        {
            DailyThresholdHours = dailyThreshold;
            WeeklyThresholdHours = weeklyThreshold;
            PremiumMultiplier = premiumMultiplier;
            HasDoubleTime = hasDoubleTime;
            DoubleTimeDailyThreshold = doubleTimeDailyThreshold;
        }
    }
}
