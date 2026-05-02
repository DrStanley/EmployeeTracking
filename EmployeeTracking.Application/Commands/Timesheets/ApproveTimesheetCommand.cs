using MediatR;

namespace EmployeeTracking.Application.Commands.Timesheets
{
    public record ApproveTimesheetCommand(
    Guid TimesheetId,
    Guid ReviewerId,
    string? Notes
) : IRequest<Unit>;
}
