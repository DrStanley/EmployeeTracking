using EmployeeTracking.Application.DTOs;
using MediatR;

namespace EmployeeTracking.Application.Queries.Timesheets
{
    public record GetClockStatus(
     Guid EmployeeId
 ) : IRequest<ClockStatusDto>;
}
