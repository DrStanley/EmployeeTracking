using EmployeeTracking.Application.DTOs;
using MediatR;

namespace EmployeeTracking.Application.Commands.Auth
{
    public record RegisterCommand(
    string FirstName,
    string LastName,
    string Email,
    string Password,
    IList<string> Roles
        ) : IRequest<AuthResponse>;
}
