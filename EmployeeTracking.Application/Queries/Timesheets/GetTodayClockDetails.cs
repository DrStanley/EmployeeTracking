using EmployeeTracking.Application.DTOs;
using MediatR;

namespace EmployeeTracking.Application.Queries.Timesheets
{
    public record GetTodayClockDetails(
     Guid EmployeeId
 ) : IRequest<IEnumerable<ClockTodayResponse>>;
}
