using EmployeeTracking.Application.Commands.ClockIn;
using EmployeeTracking.Application.DTOs;
using EmployeeTracking.Application.Interfaces;
using EmployeeTracking.Domain.Common;
using EmployeeTracking.Domain.Entities;
using EmployeeTracking.Domain.Enums;
using MediatR;

namespace EmployeeTracking.Infrastructure.Handlers
{
    public class BreakStartCommandHandler
    : IRequestHandler<BreakStartCommand, BreakStartResponse>
    {
        private readonly IUnitOfWork _uow;
        private readonly ITimeEntryFactory _factory;

        public BreakStartCommandHandler(IUnitOfWork uow, ITimeEntryFactory factory)
        {
            _uow = uow;
            _factory = factory;
        }

        public async Task<BreakStartResponse> Handle(
            BreakStartCommand request, CancellationToken ct)
        {
            // Must be clocked in to start a break
            var hasOpenPunch = await _uow.TimeEntries
                .HasOpenClockInAsync(request.EmployeeId, ct);

            if (!hasOpenPunch)
                throw new DomainException(
                    "You must be clocked in before starting a break.");

            var employee = await _uow.Employees.GetByIdAsync(request.EmployeeId, ct)
                ?? throw new NotFoundException(nameof(Employee), request.EmployeeId);

            var entry = _factory.CreateBreakStart(
                request.EmployeeId,
                TimeEntrySource.WebApp,
                DateTimeOffset.UtcNow);

            await _uow.TimeEntries.AddAsync(entry, ct);
            await _uow.SaveChangesAsync(ct);

            return new BreakStartResponse(
                EntryId: entry.Id,
                EmployeeId: entry.EmployeeId,
                Timestamp: entry.Timestamp,
                Message: $"Break started at {entry.Timestamp:HH:mm} UTC.");
        }
    }
}
