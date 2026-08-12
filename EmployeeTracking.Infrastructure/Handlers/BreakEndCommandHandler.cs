using EmployeeTracking.Application.Commands.ClockIn;
using EmployeeTracking.Application.DTOs;
using EmployeeTracking.Application.Interfaces;
using EmployeeTracking.Domain.Common;
using EmployeeTracking.Domain.Enums;
using MediatR;

namespace EmployeeTracking.Infrastructure.Handlers
{
    public class BreakEndCommandHandler
    : IRequestHandler<BreakEndCommand, BreakEndResponse>
    {
        private readonly IUnitOfWork _uow;
        private readonly ITimeEntryFactory _factory;

        public BreakEndCommandHandler(IUnitOfWork uow, ITimeEntryFactory factory)
        {
            _uow = uow;
            _factory = factory;
        }

        public async Task<BreakEndResponse> Handle(
            BreakEndCommand request, CancellationToken ct)
        {
            // Find the open break-start entry
            var entries = await _uow.TimeEntries
                .GetByEmployeeAsync(request.EmployeeId, ct);

            var openBreak = entries
                .Where(e => e.EntryType == TimeEntryType.BreakStart)
                .OrderByDescending(e => e.Timestamp)
                .FirstOrDefault();

            // Make sure there is no matching break-end after it
            if (openBreak is not null)
            {
                var hasMatchingEnd = entries.Any(e =>
                    e.EntryType == TimeEntryType.BreakEnd &&
                    e.Timestamp > openBreak.Timestamp);

                if (hasMatchingEnd)
                    openBreak = null;
            }

            if (openBreak is null)
                throw new DomainException(
                    "No open break found. Start a break first.");

            var now = DateTimeOffset.UtcNow;
            var entry = _factory.CreateBreakEnd(
                request.EmployeeId,
                TimeEntrySource.WebApp,
                now);

            var breakDuration = (decimal)(now - openBreak.Timestamp).TotalHours;

            await _uow.TimeEntries.AddAsync(entry, ct);
            await _uow.SaveChangesAsync(ct);

            return new BreakEndResponse(
                EntryId: entry.Id,
                EmployeeId: entry.EmployeeId,
                Timestamp: entry.Timestamp,
                BreakDurationHours: Math.Round(breakDuration, 2),
                Message: $"Break ended. Duration: {Math.Round(breakDuration * 60, 0)} minutes.");
        }
    }
}
