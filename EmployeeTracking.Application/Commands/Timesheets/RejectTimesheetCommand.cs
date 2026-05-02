using MediatR;

namespace EmployeeTracking.Application.Commands.Timesheets
{
    public record RejectTimesheetCommand(
    Guid TimesheetId,
    Guid ReviewerId,
    string Reason
) : IRequest<Unit>;
}
