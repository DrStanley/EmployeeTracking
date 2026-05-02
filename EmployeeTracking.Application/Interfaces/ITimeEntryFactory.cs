using EmployeeTracking.Domain.Entities;
using EmployeeTracking.Domain.Enums;
using EmployeeTracking.Domain.ValueObjects;

namespace EmployeeTracking.Application.Interfaces
{
    public interface ITimeEntryFactory
    {
        TimeEntry CreateClockIn(
            Guid employeeId,
            TimeEntrySource source,
            DateTimeOffset timestamp,
            LocationMetadata? location = null);

        TimeEntry CreateClockOut(
            Guid employeeId,
            TimeEntrySource source,
            DateTimeOffset timestamp,
            LocationMetadata? location = null);

        TimeEntry CreateBreakStart(
            Guid employeeId,
            TimeEntrySource source,
            DateTimeOffset timestamp);

        TimeEntry CreateBreakEnd(
            Guid employeeId,
            TimeEntrySource source,
            DateTimeOffset timestamp);
    }
}
