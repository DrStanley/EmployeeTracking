using EmployeeTracking.Application.DTOs;
using MediatR;

namespace EmployeeTracking.Application.Commands.RejectPTORequest
{
    public record RejectPTOCommand(
      Guid RequestId,
      Guid ReviewerId,
      string Reason
  ) : IRequest<PTORequestDto>;
}
