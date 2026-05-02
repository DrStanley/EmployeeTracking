using EmployeeTracking.Application.DTOs;
using MediatR;

namespace EmployeeTracking.Application.Commands.Timesheets
{
    public record CreateTimesheetCommand(
     Guid EmployeeId,
     Guid PayPeriodId
 ) : IRequest<TimesheetDto>;
}
