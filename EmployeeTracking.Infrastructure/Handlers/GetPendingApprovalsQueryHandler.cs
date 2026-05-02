using EmployeeTracking.Application.DTOs;
using EmployeeTracking.Application.Interfaces;
using EmployeeTracking.Application.Queries.Timesheets;
using MediatR;

namespace EmployeeTracking.Infrastructure.Handlers
{
    public class GetPendingApprovalsQueryHandler
        : IRequestHandler<GetPendingApprovalsQuery, IReadOnlyList<TimesheetDto>>
    {
        private readonly IUnitOfWork _uow;

        public GetPendingApprovalsQueryHandler(IUnitOfWork uow) => _uow = uow;

        public async Task<IReadOnlyList<TimesheetDto>> Handle(
            GetPendingApprovalsQuery request, CancellationToken ct)
        {
            var timesheets = await _uow.Timesheets
                .GetPendingApprovalsForManagerAsync(request.ManagerId, ct);

            var result = new List<TimesheetDto>();

            foreach (var timesheet in timesheets)
            {
                var employee = await _uow.Employees.GetByIdAsync(timesheet.EmployeeId, ct);
                var period = await _uow.PayPeriods.GetByIdAsync(timesheet.PayPeriodId, ct);

                result.Add(new TimesheetDto(
                    Id: timesheet.Id,
                    EmployeeId: timesheet.EmployeeId,
                    EmployeeFullName: employee?.FullName ?? "Unknown",
                    PayPeriodId: timesheet.PayPeriodId,
                    PayPeriodName: period?.Name ?? "Unknown",
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
                    ApprovalHistory: new List<ApprovalActionDto>()));
            }

            return result;
        }
    }
}
