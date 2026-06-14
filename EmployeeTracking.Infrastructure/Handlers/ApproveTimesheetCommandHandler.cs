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
        private readonly IEmailNotificationService _emailService;

        public ApproveTimesheetCommandHandler(
            IUnitOfWork uow,
            IEmailNotificationService emailService)
        {
            _uow = uow;
            _emailService = emailService;
        }

        public async Task<Unit> Handle(
            ApproveTimesheetCommand request, CancellationToken ct)
        {
            var timesheet = await _uow.Timesheets.GetByIdAsync(request.TimesheetId, ct)
                ?? throw new NotFoundException(nameof(Timesheet), request.TimesheetId);

            var employee = await _uow.Employees.GetByIdAsync(timesheet.EmployeeId, ct)!;
            if (employee!.ManagerId != request.ReviewerId)
                throw new UnauthorizedAccessException(
                    "Only the employee's assigned manager can approve this timesheet.");

            var period = await _uow.PayPeriods.GetByIdAsync(timesheet.PayPeriodId, ct)!;

            timesheet.Approve(request.ReviewerId, request.Notes);
            _uow.Timesheets.Update(timesheet);
            await _uow.SaveChangesAsync(ct);

            // Send approval email — fire and forget so it doesn't block the response
            _ = _emailService.SendTimesheetApprovedAsync(
                timesheet, employee!, period!, ct);

            return Unit.Value;
        }
    }
}
