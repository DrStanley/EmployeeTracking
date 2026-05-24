using EmployeeTracking.Application.DTOs;
using MediatR;

namespace EmployeeTracking.Application.Commands.CancelPTORequest
{
    public record CancelPTOCommand(
     Guid RequestId,
     Guid EmployeeId
 ) : IRequest<PTORequestDto>;
}
