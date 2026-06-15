using MediatR;

namespace EmployeeTracking.Application.Commands.Auth
{
    public record RevokeTokenCommand(
        string RefreshToken
    ) : IRequest<Unit>;
}
