using EmployeeTracking.Application.DTOs;
using MediatR;

namespace EmployeeTracking.Application.Commands.SubmitPTORequest
{
    public record SubmitPTOCommand(
    Guid EmployeeId,
    DateOnly StartDate,
    DateOnly EndDate,
    decimal HoursRequested,
    string? Notes
) : IRequest<PTORequestDto>;
}
