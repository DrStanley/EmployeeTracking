using EmployeeTracking.Application.DTOs;
using EmployeeTracking.Domain.Entities;
using EmployeeTracking.Domain.Enums;

namespace EmployeeTracking.Application.Time
{
    public static class ClockStateReader
    {
        public static ClockStatusDto Read(IEnumerable<TimeEntry> entries)
        {
            var list = entries.ToList();
            var clockIns = list
                .Where(e => e.EntryType == TimeEntryType.ClockIn)
                .OrderByDescending(e => e.Timestamp)
                .ToList();
            var clockOuts = list
                .Where(e => e.EntryType == TimeEntryType.ClockOut)
                .OrderByDescending(e => e.Timestamp)
                .ToList();

            var hasOpenPunch = clockIns.Count > 0 &&
                (clockOuts.Count == 0 || clockIns[0].Timestamp > clockOuts[0].Timestamp);

            if (!hasOpenPunch)
                return new ClockStatusDto("clocked-out", null, null);

            var openClockIn = clockIns[0];
            var breakStarts = list
                .Where(e => e.EntryType == TimeEntryType.BreakStart && e.Timestamp > openClockIn.Timestamp)
                .OrderByDescending(e => e.Timestamp)
                .ToList();
            var breakEnds = list
                .Where(e => e.EntryType == TimeEntryType.BreakEnd && e.Timestamp > openClockIn.Timestamp)
                .OrderByDescending(e => e.Timestamp)
                .ToList();
            var onBreak = breakStarts.Count > 0 &&
                (breakEnds.Count == 0 || breakStarts[0].Timestamp > breakEnds[0].Timestamp);

            return new ClockStatusDto(
                onBreak ? "on-break" : "clocked-in",
                openClockIn.Timestamp,
                onBreak ? breakStarts[0].Timestamp : null);
        }
    }
}
