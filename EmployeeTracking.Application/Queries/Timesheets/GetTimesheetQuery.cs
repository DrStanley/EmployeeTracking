using EmployeeTracking.Application.DTOs;
using MediatR;

namespace EmployeeTracking.Application.Queries.Timesheets
{
    public record GetTimesheetQuery(
     Guid EmployeeId,
     Guid PayPeriodId
 ) : IRequest<TimesheetDto>;
}
