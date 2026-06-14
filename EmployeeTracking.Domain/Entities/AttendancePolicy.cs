using EmployeeTracking.Domain.Common;

namespace EmployeeTracking.Domain.Entities
{
    public class AttendancePolicy : AuditableEntity
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; } = string.Empty;
        public decimal DailyOvertimeThresholdHours { get; private set; } = 8m;
        public decimal WeeklyOvertimeThresholdHours { get; private set; } = 40m;
        public decimal OvertimeMultiplier { get; private set; } = 1.5m;
        public bool HasPaidBreaks { get; private set; }
        public int PaidBreakMinutes { get; private set; }
        public int UnpaidBreakMinutes { get; private set; }

        // PTO accrual
        public decimal PTOAccrualRatePerPayPeriod { get; private set; }
        public decimal MaxPTOBalanceHours { get; private set; }

        private AttendancePolicy() { }

        public static AttendancePolicy CreateDefault(string name) =>
            new()
            {
                Id = Guid.NewGuid(),
                Name = name,
                DailyOvertimeThresholdHours = 8m,
                WeeklyOvertimeThresholdHours = 40m,
                OvertimeMultiplier = 1.5m,
                HasPaidBreaks = false,
                PaidBreakMinutes = 0,
                UnpaidBreakMinutes = 30,
                PTOAccrualRatePerPayPeriod = 4m,
                MaxPTOBalanceHours = 240m
            };
        public void Update(
    string name,
    decimal dailyOT,
    decimal weeklyOT,
    decimal multiplier,
    bool hasPaidBreaks,
    int paidBreakMinutes,
    int unpaidBreakMinutes)
        {
            Name = name;
            DailyOvertimeThresholdHours = dailyOT;
            WeeklyOvertimeThresholdHours = weeklyOT;
            OvertimeMultiplier = multiplier;
            HasPaidBreaks = hasPaidBreaks;
            PaidBreakMinutes = paidBreakMinutes;
            UnpaidBreakMinutes = unpaidBreakMinutes;
        }

        public void UpdatePTOSettings(
            decimal accrualRate,
            decimal maxBalance)
        {
            PTOAccrualRatePerPayPeriod = accrualRate;
            MaxPTOBalanceHours = maxBalance;
        }
    }
}
