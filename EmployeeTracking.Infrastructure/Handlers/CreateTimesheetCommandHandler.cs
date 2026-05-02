using EmployeeTracking.Application.Commands.Timesheets;
using EmployeeTracking.Application.DTOs;
using EmployeeTracking.Application.Interfaces;
using EmployeeTracking.Domain.Common;
using EmployeeTracking.Domain.Entities;
using MediatR;

namespace EmployeeTracking.Infrastructure.Handlers
{
    public class CreateTimesheetCommandHandler
     : IRequestHandler<CreateTimesheetCommand, TimesheetDto>
    {
        private readonly IUnitOfWork _uow;
        private readonly ITimesheetCalculationService _calculator;

        public CreateTimesheetCommandHandler(
            IUnitOfWork uow,
            ITimesheetCalculationService calculator)
        {
            _uow = uow;
            _calculator = calculator;
        }

        public async Task<TimesheetDto> Handle(
            CreateTimesheetCommand request, CancellationToken ct)
        {
            var employee = await _uow.Employees.GetByIdAsync(request.EmployeeId, ct)
                ?? throw new NotFoundException(nameof(Employee), request.EmployeeId);

            var period = await _uow.PayPeriods.GetByIdAsync(request.PayPeriodId, ct)
                ?? throw new NotFoundException(nameof(PayPeriod), request.PayPeriodId);

            // Return existing draft if already created
            var existing = await _uow.Timesheets
                .GetByEmployeeAndPeriodAsync(request.EmployeeId, request.PayPeriodId, ct);

            if (existing is not null)
                return await MapToDto(existing, employee, period, ct);

            // Calculate totals from time entries
            var (regular, overtime, pto, unpaid) = await _calculator
                .CalculateTotalsAsync(request.EmployeeId, period, ct);

            var timesheet = Timesheet.CreateForPeriod(
                request.EmployeeId, request.PayPeriodId);

            timesheet.CalculateTotals(regular, overtime, pto, unpaid);

            await _uow.Timesheets.AddAsync(timesheet, ct);
            await _uow.SaveChangesAsync(ct);

            return await MapToDto(timesheet, employee, period, ct);
        }

        private async Task<TimesheetDto> MapToDto(
            Timesheet timesheet,
            Employee employee,
            PayPeriod period,
            CancellationToken ct)
        {
            var approvalDtos = new List<ApprovalActionDto>();

            foreach (var action in timesheet.ApprovalActions)
            {
                var reviewer = await _uow.Employees.GetByIdAsync(action.ReviewerId, ct);
                approvalDtos.Add(new ApprovalActionDto(
                    action.ReviewerId,
                    reviewer?.FullName ?? "Unknown",
                    action.Decision,
                    action.Notes,
                    action.DecidedAt));
            }

            return new TimesheetDto(
                Id: timesheet.Id,
                EmployeeId: timesheet.EmployeeId,
                EmployeeFullName: employee.FullName,
                PayPeriodId: timesheet.PayPeriodId,
                PayPeriodName: period.Name,
                Status: timesheet.Status,
                TotalRegularHours: timesheet.TotalRegularHours,
                TotalOvertimeHours: timesheet.TotalOvertimeHours,
                TotalPTOHours: timesheet.TotalPTOHours,
                TotalUnpaidHours: timesheet.TotalUnpaidHours,
                SubmittedAt: timesheet.SubmittedAt,
                ApprovedAt: timesheet.ApprovedAt,
                RejectionReason: timesheet.RejectionReason,
                Lines: timesheet.Lines.Select(l => new TimesheetLineDto(
                    l.Id, l.WorkDate, l.RegularHours,
                    l.OvertimeHours, l.BreakHours, l.PTOHours, l.Notes)).ToList(),
                ApprovalHistory: approvalDtos);
        }
    }
}
