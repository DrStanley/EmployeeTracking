using EmployeeTracking.Application.Commands.Timesheets;
using EmployeeTracking.Application.Interfaces;
using EmployeeTracking.Domain.Common;
using EmployeeTracking.Domain.Entities;
using MediatR;

namespace EmployeeTracking.Infrastructure.Handlers
{
    public class ApproveTimesheetCommandHandler
    : IRequestHandler<ApproveTimesheetCommand, Unit>
    {
        private readonly IUnitOfWork _uow;

        public ApproveTimesheetCommandHandler(IUnitOfWork uow) => _uow = uow;

        public async Task<Unit> Handle(
            ApproveTimesheetCommand request, CancellationToken ct)
        {
            var timesheet = await _uow.Timesheets.GetByIdAsync(request.TimesheetId, ct)
                ?? throw new NotFoundException(nameof(Timesheet), request.TimesheetId);

            var reviewer = await _uow.Employees.GetByIdAsync(request.ReviewerId, ct)
                ?? throw new NotFoundException(nameof(Employee), request.ReviewerId);

            // Reviewer must be the employee's manager or an Admin
            var employee = await _uow.Employees.GetByIdAsync(timesheet.EmployeeId, ct)!;
            if (employee!.ManagerId != request.ReviewerId)
                throw new UnauthorizedAccessException(
                    "Only the employee's assigned manager can approve this timesheet.");

            timesheet.Approve(request.ReviewerId, request.Notes);
            _uow.Timesheets.Update(timesheet);
            await _uow.SaveChangesAsync(ct);

            return Unit.Value;
        }
    }
}
