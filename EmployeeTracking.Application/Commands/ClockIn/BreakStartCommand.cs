using EmployeeTracking.Application.DTOs;
using MediatR;

namespace EmployeeTracking.Application.Commands.ClockIn
{
    public record BreakStartCommand(
        Guid EmployeeId,
        bool IsPaid = false
    ) : IRequest<BreakStartResponse>;

}
