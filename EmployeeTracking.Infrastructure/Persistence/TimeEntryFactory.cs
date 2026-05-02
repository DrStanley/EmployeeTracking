using EmployeeTracking.Application.Interfaces;
using EmployeeTracking.Domain.Entities;
using EmployeeTracking.Domain.Enums;
using EmployeeTracking.Domain.ValueObjects;

namespace EmployeeTracking.Infrastructure.Persistence
{
    public class TimeEntryFactory : ITimeEntryFactory
    {
        public TimeEntry CreateClockIn(
            Guid employeeId,
            TimeEntrySource source,
            DateTimeOffset timestamp,
            LocationMetadata? location = null)
            => TimeEntry.Create(employeeId, TimeEntryType.ClockIn, source, timestamp, location);

        public TimeEntry CreateClockOut(
            Guid employeeId,
            TimeEntrySource source,
            DateTimeOffset timestamp,
            LocationMetadata? location = null)
            => TimeEntry.Create(employeeId, TimeEntryType.ClockOut, source, timestamp, location);

        public TimeEntry CreateBreakStart(
            Guid employeeId,
            TimeEntrySource source,
            DateTimeOffset timestamp)
            => TimeEntry.Create(employeeId, TimeEntryType.BreakStart, source, timestamp);

        public TimeEntry CreateBreakEnd(
            Guid employeeId,
            TimeEntrySource source,
            DateTimeOffset timestamp)
            => TimeEntry.Create(employeeId, TimeEntryType.BreakEnd, source, timestamp);
    }
}
