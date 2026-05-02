using EmployeeTracking.Application.Commands.Timesheets;
using EmployeeTracking.Application.Interfaces;
using EmployeeTracking.Domain.Common;
using EmployeeTracking.Domain.Entities;
using MediatR;

namespace EmployeeTracking.Infrastructure.Handlers
{
    public class SubmitTimesheetCommandHandler
     : IRequestHandler<SubmitTimesheetCommand, Unit>
    {
        private readonly IUnitOfWork _uow;

        public SubmitTimesheetCommandHandler(IUnitOfWork uow) => _uow = uow;

        public async Task<Unit> Handle(
            SubmitTimesheetCommand request, CancellationToken ct)
        {
            var timesheet = await _uow.Timesheets.GetByIdAsync(request.TimesheetId, ct)
                ?? throw new NotFoundException(nameof(Timesheet), request.TimesheetId);

            if (timesheet.EmployeeId != request.EmployeeId)
                throw new UnauthorizedAccessException(
                    "You can only submit your own timesheet.");

            timesheet.Submit();
            _uow.Timesheets.Update(timesheet);
            await _uow.SaveChangesAsync(ct);

            return Unit.Value;
        }
    }
}
