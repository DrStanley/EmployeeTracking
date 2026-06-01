using EmployeeTracking.Application.DTOs;
using EmployeeTracking.Application.Interfaces;

namespace EmployeeTracking.Infrastructure.Persistence
{

    /// <summary>
    /// Standard FLSA overtime — daily OT after threshold,
    /// then weekly OT after weekly threshold on top.
    /// </summary>
    public class StandardOvertimeStrategy : IOvertimeStrategy
    {
        public (decimal regular, decimal overtime) Calculate(
            IReadOnlyList<DailyHoursDto> dailyHours,
            decimal dailyThreshold,
            decimal weeklyThreshold)
        {
            decimal totalRegular = 0m;
            decimal totalOvertime = 0m;

            // daily overtime
            foreach (var day in dailyHours)
            {
                var dailyRegular = Math.Min(day.WorkedHours, dailyThreshold);
                var dailyOvertime = Math.Max(0m, day.WorkedHours - dailyThreshold);

                totalRegular += dailyRegular;
                totalOvertime += dailyOvertime;
            }

            //weekly overtime on top of daily
            var weeklyTotal = dailyHours.Sum(d => d.WorkedHours);
            if (weeklyTotal > weeklyThreshold)
            {
                var additionalWeeklyOT = Math.Max(
                    0m, weeklyTotal - weeklyThreshold - totalOvertime);

                totalOvertime += additionalWeeklyOT;
                totalRegular -= additionalWeeklyOT;
            }

            return (Math.Max(0m, totalRegular), totalOvertime);
        }
    }
}
