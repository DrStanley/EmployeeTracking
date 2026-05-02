using EmployeeTracking.Application.DTOs;
using MediatR;

namespace EmployeeTracking.Application.Queries.Timesheets
{
    public record GetPendingApprovalsQuery(
        Guid ManagerId
    ) : IRequest<IReadOnlyList<TimesheetDto>>;
}
