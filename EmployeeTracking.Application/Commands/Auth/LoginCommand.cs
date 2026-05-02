using EmployeeTracking.Application.DTOs;
using MediatR;

namespace EmployeeTracking.Application.Commands.Auth
{

    public record LoginCommand(
        string Email,
        string Password
    ) : IRequest<AuthResponse>;
}
