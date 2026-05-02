using EmployeeTracking.Application.Commands.ClockOut;
using EmployeeTracking.Application.DTOs;
using EmployeeTracking.Application.Interfaces;
using EmployeeTracking.Domain.Common;
using EmployeeTracking.Domain.Entities;
using EmployeeTracking.Domain.ValueObjects;
using MediatR;

namespace EmployeeTracking.Infrastructure.Handlers
{
    public class ClockOutCommandHandler : IRequestHandler<ClockOutCommand, ClockOutResponse>
    {
        private readonly IUnitOfWork _uow;
        private readonly ITimeEntryFactory _factory;

        public ClockOutCommandHandler(IUnitOfWork uow, ITimeEntryFactory factory)
        {
            _uow = uow;
            _factory = factory;
        }

        public async Task<ClockOutResponse> Handle(
            ClockOutCommand request, CancellationToken ct)
        {
            // 1. Verify employee exists
            var employee = await _uow.Employees.GetByIdAsync(request.EmployeeId, ct)
                ?? throw new NotFoundException(nameof(Employee), request.EmployeeId);

            // 2. Must have an open clock-in to clock out
            var hasOpenPunch = await _uow.TimeEntries
                .HasOpenClockInAsync(request.EmployeeId, ct);

            if (!hasOpenPunch)
                throw new DomainException(
                    "No open clock-in found. Please clock in first.");

            // 3. Get the open clock-in to calculate hours worked
            var openClockIn = await _uow.TimeEntries
                .GetLatestOpenClockInAsync(request.EmployeeId, ct)
                ?? throw new DomainException("Could not retrieve open clock-in entry.");

            // 4. Build location metadata
            LocationMetadata? location = null;
            if (request.Latitude.HasValue && request.Longitude.HasValue)
            {
                location = new LocationMetadata(
                    request.Latitude,
                    request.Longitude,
                    request.DeviceId,
                    null);
            }

            // 5. Create the clock-out entry
            var now = DateTimeOffset.UtcNow;
            var entry = _factory.CreateClockOut(
                request.EmployeeId,
                request.Source,
                now,
                location);

            var hoursWorked = (decimal)(now - openClockIn.Timestamp).TotalHours;

            await _uow.TimeEntries.AddAsync(entry, ct);
            await _uow.SaveChangesAsync(ct);

            return new ClockOutResponse(
                EntryId: entry.Id,
                EmployeeId: entry.EmployeeId,
                Timestamp: entry.Timestamp,
                HoursWorked: Math.Round(hoursWorked, 2),
                Message: $"Clocked out successfully. Hours worked: {Math.Round(hoursWorked, 2)}h.");
        }
    }
}
