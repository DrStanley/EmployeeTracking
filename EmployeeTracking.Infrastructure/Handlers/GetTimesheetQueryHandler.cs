using EmployeeTracking.Application.DTOs;
using EmployeeTracking.Application.Interfaces;
using EmployeeTracking.Application.Queries.Timesheets;
using EmployeeTracking.Domain.Common;
using MediatR;

namespace EmployeeTracking.Infrastructure.Handlers
{
    public class GetTimesheetQueryHandler
    : IRequestHandler<GetTimesheetQuery, TimesheetDto>
    {
        private readonly IUnitOfWork _uow;

        public GetTimesheetQueryHandler(IUnitOfWork uow) => _uow = uow;

        public async Task<TimesheetDto> Handle(
            GetTimesheetQuery request, CancellationToken ct)
        {
            var timesheet = await _uow.Timesheets
                .GetByEmployeeAndPeriodAsync(request.EmployeeId, request.PayPeriodId, ct)
                ?? throw new NotFoundException(
                    "Timesheet", $"{request.EmployeeId}/{request.PayPeriodId}");

            var employee = await _uow.Employees.GetByIdAsync(timesheet.EmployeeId, ct)!;
            var period = await _uow.PayPeriods.GetByIdAsync(timesheet.PayPeriodId, ct)!;

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
                EmployeeFullName: employee!.FullName,
                PayPeriodId: timesheet.PayPeriodId,
                PayPeriodName: period!.Name,
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
