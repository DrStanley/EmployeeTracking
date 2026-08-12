using EmployeeTracking.Application.Commands.ClockOut;
using EmployeeTracking.Application.DTOs;
using EmployeeTracking.Application.Interfaces;
using EmployeeTracking.Domain.Common;
using EmployeeTracking.Domain.Entities;
using EmployeeTracking.Domain.ValueObjects;
using MediatR;

namespace EmployeeTracking.Infrastructure.Handlers;

public class ClockOutCommandHandler : IRequestHandler<ClockOutCommand, ClockOutResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly ITimeEntryFactory _factory;
    private readonly ITimesheetCalculationService _calculator;

    public ClockOutCommandHandler(
        IUnitOfWork uow,
        ITimeEntryFactory factory,
        ITimesheetCalculationService calculator)
    {
        _uow = uow;
        _factory = factory;
        _calculator = calculator;
    }

    public async Task<ClockOutResponse> Handle(
        ClockOutCommand request, CancellationToken ct)
    {
        // 1. Verify employee exists
        var employee = await _uow.Employees.GetByIdAsync(request.EmployeeId, ct)
            ?? throw new NotFoundException(nameof(Employee), request.EmployeeId);

        // 2. Must have an open clock-in
        var hasOpenPunch = await _uow.TimeEntries
            .HasOpenClockInAsync(request.EmployeeId, ct);

        if (!hasOpenPunch)
            throw new DomainException("No open clock-in found. Please clock in first.");

        var openClockIn = await _uow.TimeEntries
            .GetLatestOpenClockInAsync(request.EmployeeId, ct)
            ?? throw new DomainException("Could not retrieve open clock-in entry.");

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

        // 4. Save the clock-out entry
        var now = DateTimeOffset.UtcNow;
        var entry = _factory.CreateClockOut(
            request.EmployeeId,
            request.Source,
            now,
            location);

        var hoursWorked = (decimal)(now - openClockIn.Timestamp).TotalHours;

        await _uow.TimeEntries.AddAsync(entry, ct);
        await _uow.SaveChangesAsync(ct);

        // 5. Auto-update the timesheet for the current pay period
        await RecalculateTimesheetAsync(request.EmployeeId, ct);

        return new ClockOutResponse(
            EntryId: entry.Id,
            EmployeeId: entry.EmployeeId,
            Timestamp: entry.Timestamp,
            HoursWorked: Math.Round(hoursWorked, 2),
            Message: $"Clocked out. Hours this session: {Math.Round(hoursWorked, 2)}h.");
    }

    private async Task RecalculateTimesheetAsync(Guid employeeId, CancellationToken ct)
    {
        // Find the current pay period
        var period = await _uow.PayPeriods.GetCurrentAsync(ct);
        if (period is null) return; // no current period configured yet

        // Get or create the timesheet for this period
        var timesheet = await _uow.Timesheets
            .GetByEmployeeAndPeriodAsync(employeeId, period.Id, ct);

        if (timesheet is null)
        {
            timesheet = Timesheet.CreateForPeriod(employeeId, period.Id);
            await _uow.Timesheets.AddAsync(timesheet, ct);
        }

        // Don't recalculate locked or approved timesheets
        if (timesheet.Status == Domain.Enums.TimesheetStatus.Locked ||
            timesheet.Status == Domain.Enums.TimesheetStatus.Approved)
            return;

        // Recalculate totals from all time entries
        var (regular, overtime, pto, unpaid) = await _calculator
            .CalculateTotalsAsync(employeeId, period, ct);

        timesheet.CalculateTotals(regular, overtime, pto, unpaid);
        _uow.Timesheets.Update(timesheet);
        await _uow.SaveChangesAsync(ct);
    }
}