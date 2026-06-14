using EmployeeTracking.Application.Commands.ClockIn;
using EmployeeTracking.Application.DTOs;
using EmployeeTracking.Application.Interfaces;
using EmployeeTracking.Domain.Common;
using EmployeeTracking.Domain.Entities;
using EmployeeTracking.Domain.ValueObjects;
using MediatR;

namespace EmployeeTracking.Infrastructure.Handlers
{

    public class ClockInCommandHandler : IRequestHandler<ClockInCommand, ClockInResponse>
    {
        private readonly IUnitOfWork _uow;
        private readonly ITimeEntryFactory _factory;

        public ClockInCommandHandler(IUnitOfWork uow, ITimeEntryFactory factory)
        {
            _uow = uow;
            _factory = factory;
        }

        public async Task<ClockInResponse> Handle(
            ClockInCommand request, CancellationToken ct)
        {
            // 1. Verify employee exists and is active
            var employee = await _uow.Employees.GetByIdAsync(request.EmployeeId, ct)
                ?? throw new NotFoundException(nameof(Employee), request.EmployeeId);

            if (!employee.IsActive)
                throw new DomainException("Inactive employees cannot clock in.");

            // 2. Prevent double punch
            var hasOpenPunch = await _uow.TimeEntries
                .HasOpenClockInAsync(request.EmployeeId, ct);

            if (hasOpenPunch)
                throw new DomainException(
                    "You already have an open clock-in. Please clock out first.");

            // 3. Build location metadata
            LocationMetadata? location = null;
            if (request.Latitude.HasValue && request.Longitude.HasValue)
            {
                location = new LocationMetadata(
                    request.Latitude,
                    request.Longitude,
                    request.DeviceId,
                    null);
            }

            // 4. Create the time entry
            var entry = _factory.CreateClockIn(
                request.EmployeeId,
                request.Source,
                DateTimeOffset.Now,
                location);

            await _uow.TimeEntries.AddAsync(entry, ct);
            await _uow.SaveChangesAsync(ct);

            return new ClockInResponse(
                EntryId: entry.Id,
                EmployeeId: entry.EmployeeId,
                Timestamp: entry.Timestamp,
                Message: $"Clocked in successfully at {entry.Timestamp:HH:mm} UTC.");
        }
    }
}
