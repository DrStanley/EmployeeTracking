using MediatR;

namespace EmployeeTracking.Application.Commands.Timesheets
{
    public record SubmitTimesheetCommand(
      Guid TimesheetId,
      Guid EmployeeId
  ) : IRequest<Unit>;
}
