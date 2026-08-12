using EmployeeTracking.Application.DTOs;
using MediatR;

namespace EmployeeTracking.Application.Commands.ClockIn
{
    public record BreakEndCommand(
    Guid EmployeeId
) : IRequest<BreakEndResponse>;
  
}
