using EmployeeTracking.Application.DTOs;
using EmployeeTracking.Application.Interfaces;
using EmployeeTracking.Application.Queries.Timesheets;
using EmployeeTracking.Domain.Enums;
using MediatR;

namespace EmployeeTracking.Infrastructure.Handlers
{
    public class GetClockStatusHandler
    : IRequestHandler<GetClockStatus, ClockStatusDto>
    {
        private readonly IUnitOfWork _uow;

        public GetClockStatusHandler(IUnitOfWork uow) => _uow = uow;

        public async Task<ClockStatusDto> Handle(
            GetClockStatus request, CancellationToken ct)
        {
            var entries = await _uow.TimeEntries
           .GetByEmployeeAsync(request.EmployeeId, ct);

            // Find latest clock-in with no matching clock-out
            var clockIns = entries
                .Where(e => e.EntryType == TimeEntryType.ClockIn)
                .OrderByDescending(e => e.Timestamp)
                .ToList();

            var clockOuts = entries
                .Where(e => e.EntryType == TimeEntryType.ClockOut)
                .OrderByDescending(e => e.Timestamp)
                .ToList();

            var hasOpenPunch = clockIns.Any() &&
                (!clockOuts.Any() || clockIns[0].Timestamp > clockOuts[0].Timestamp);

            if (!hasOpenPunch)
                return new ClockStatusDto("clocked-out", null, null);

            var openClockIn = clockIns[0];

            // Check for open break
            var breakStarts = entries
                .Where(e => e.EntryType == TimeEntryType.BreakStart
                         && e.Timestamp > openClockIn.Timestamp)
                .OrderByDescending(e => e.Timestamp)
                .ToList();

            var breakEnds = entries
                .Where(e => e.EntryType == TimeEntryType.BreakEnd
                         && e.Timestamp > openClockIn.Timestamp)
                .OrderByDescending(e => e.Timestamp)
                .ToList();

            var onBreak = breakStarts.Any() &&
                (!breakEnds.Any() || breakStarts[0].Timestamp > breakEnds[0].Timestamp);


            return new ClockStatusDto(
                Status: onBreak ? "on-break" : "clocked-in",
                ClockedInAt: openClockIn.Timestamp,
                BreakStartedAt: onBreak ? breakStarts[0].Timestamp : null
            );
        }
    }
}
