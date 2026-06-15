using MediatR;

namespace EmployeeTracking.Application.Commands.Auth
{
    public record RevokeAllTokensCommand(
        string UserId
    ) : IRequest<Unit>;
}
