using EmployeeTracking.Application.Commands.Timesheets;
using EmployeeTracking.Application.Interfaces;
using EmployeeTracking.Domain.Common;
using EmployeeTracking.Domain.Entities;
using MediatR;

namespace EmployeeTracking.Infrastructure.Handlers
{
    public class RejectTimesheetCommandHandler
     : IRequestHandler<RejectTimesheetCommand, Unit>
    {
        private readonly IUnitOfWork _uow;

        public RejectTimesheetCommandHandler(IUnitOfWork uow) => _uow = uow;

        public async Task<Unit> Handle(
            RejectTimesheetCommand request, CancellationToken ct)
        {
            var timesheet = await _uow.Timesheets.GetByIdAsync(request.TimesheetId, ct)
                ?? throw new NotFoundException(nameof(Timesheet), request.TimesheetId);

            var employee = await _uow.Employees.GetByIdAsync(timesheet.EmployeeId, ct)!;
            if (employee!.ManagerId != request.ReviewerId)
                throw new UnauthorizedAccessException(
                    "Only the employee's assigned manager can reject this timesheet.");

            timesheet.Reject(request.ReviewerId, request.Reason);
            _uow.Timesheets.Update(timesheet);
            await _uow.SaveChangesAsync(ct);

            return Unit.Value;
        }
    }
}
