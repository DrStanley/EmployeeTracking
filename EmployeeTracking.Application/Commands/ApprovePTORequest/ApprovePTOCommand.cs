using EmployeeTracking.Application.DTOs;
using MediatR;

namespace EmployeeTracking.Application.Commands.ApprovePTORequest
{
    public record ApprovePTOCommand(
    Guid RequestId,
    Guid ReviewerId,
    string? Notes
) : IRequest<PTORequestDto>;
}
