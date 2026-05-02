using MediatR;

namespace EmployeeTracking.Application.Queries.Auth
{
    public record GetCurrentUserQuery(string Email) : IRequest<CurrentUserResponse>;

    public record CurrentUserResponse(
        string Email,
        Guid EmployeeId,
        IList<string> Roles
    );
}
