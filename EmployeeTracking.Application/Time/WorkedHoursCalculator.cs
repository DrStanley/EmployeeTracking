using EmployeeTracking.Domain.Entities;
using EmployeeTracking.Domain.Enums;

namespace EmployeeTracking.Application.Time
{
    public static class WorkedHoursCalculator
    {
        public static IReadOnlyList<(DateTimeOffset Start, decimal Hours)> Pairs(IEnumerable<TimeEntry> entries)
        {
            var sorted = entries.OrderBy(e => e.Timestamp).ToList();
            var result = new List<(DateTimeOffset Start, decimal Hours)>();

            foreach (var clockIn in sorted.Where(e => e.EntryType == TimeEntryType.ClockIn))
            {
                var clockOut = sorted.FirstOrDefault(e =>
                    e.EntryType == TimeEntryType.ClockOut &&
                    e.Timestamp > clockIn.Timestamp &&
                    !sorted.Any(other =>
                        other.EntryType == TimeEntryType.ClockIn &&
                        other.Timestamp > clockIn.Timestamp &&
                        other.Timestamp < e.Timestamp));

                if (clockOut is null)
                    continue;

                var hours = (decimal)(clockOut.Timestamp - clockIn.Timestamp).TotalHours;
                foreach (var breakStart in sorted.Where(e =>
                    e.EntryType == TimeEntryType.BreakStart &&
                    e.Timestamp > clockIn.Timestamp &&
                    e.Timestamp < clockOut.Timestamp))
                {
                    var breakEnd = sorted.FirstOrDefault(e =>
                        e.EntryType == TimeEntryType.BreakEnd &&
                        e.Timestamp > breakStart.Timestamp &&
                        e.Timestamp < clockOut.Timestamp);
                    if (breakEnd is not null)
                        hours -= (decimal)(breakEnd.Timestamp - breakStart.Timestamp).TotalHours;
                }

                result.Add((clockIn.Timestamp, Math.Max(0m, Math.Round(hours, 2, MidpointRounding.AwayFromZero))));
            }

            return result;
        }

        public static decimal Total(IEnumerable<TimeEntry> entries) =>
            Math.Round(Pairs(entries).Sum(pair => pair.Hours), 2, MidpointRounding.AwayFromZero);

        public static IReadOnlyDictionary<DateOnly, decimal> ByLocalDay(IEnumerable<TimeEntry> entries)
        {
            var map = new Dictionary<DateOnly, decimal>();
            foreach (var pair in Pairs(entries))
            {
                var day = DateOnly.FromDateTime(pair.Start.LocalDateTime);
                map[day] = Math.Round(map.GetValueOrDefault(day) + pair.Hours, 2, MidpointRounding.AwayFromZero);
            }
            return map;
        }
    }
}
