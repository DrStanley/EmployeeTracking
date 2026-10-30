using EmployeeTracking.Application.DTOs;
using EmployeeTracking.Domain.Common;
using EmployeeTracking.Domain.Entities;

namespace EmployeeTracking.Application.Activity
{
    public static class AppActivityCalculator
    {
        public static void EnsureRange(DateTimeOffset from, DateTimeOffset to)
        {
            if (to <= from)
                throw new DomainException("The end of the activity range must be after the start.");
            if (to - from > TimeSpan.FromDays(93))
                throw new DomainException("Activity can be viewed for at most 93 days at a time.");
        }

        public static decimal Hours(int seconds) =>
            Math.Round(seconds / 3600m, 2, MidpointRounding.AwayFromZero);

        public static (decimal TotalHours, IReadOnlyList<AppUsageItemDto> Apps) Summarize(
            IEnumerable<AppUsageSession> sessions)
        {
            var list = sessions.ToList();
            var totalSeconds = list.Sum(s => s.DurationSeconds);

            var apps = list
                .GroupBy(s => s.AppName.Trim(), StringComparer.OrdinalIgnoreCase)
                .Select(g =>
                {
                    var seconds = g.Sum(s => s.DurationSeconds);
                    return new AppUsageItemDto(
                        AppName: g.OrderByDescending(s => s.StartedAt).First().AppName.Trim(),
                        Hours: Hours(seconds),
                        SessionCount: g.Count(),
                        LastUsedAt: g.Max(s => s.EndedAt),
                        Percent: totalSeconds == 0
                            ? 0
                            : Math.Round(seconds * 100m / totalSeconds, 1, MidpointRounding.AwayFromZero));
                })
                .OrderByDescending(a => a.Hours)
                .ThenBy(a => a.AppName, StringComparer.OrdinalIgnoreCase)
                .ToList();

            return (Hours(totalSeconds), apps);
        }
    }
}
