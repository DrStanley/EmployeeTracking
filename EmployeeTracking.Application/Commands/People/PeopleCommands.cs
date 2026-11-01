using MediatR;

namespace EmployeeTracking.Application.Commands.People
{
    public record PersonAccountDto(
        string UserId,
        Guid EmployeeId,
        string Email,
        string FullName,
        string JobTitle,
        string Role,
        bool IsAdmin,
        string AccountStatus);

    public record ListPeopleQuery : IRequest<IReadOnlyList<PersonAccountDto>>;

    public record SetPersonRoleRequest(string Role);

    public record SetPersonRoleCommand(string UserId, string Role) : IRequest<PersonAccountDto>;

    public record SetPersonStatusRequest(string AccountStatus);

    public record SetPersonStatusCommand(string UserId, string AccountStatus) : IRequest<PersonAccountDto>;

    public record AddPersonCommand(
        string FirstName,
        string LastName,
        string Email,
        string Password,
        string EmployeeNumber,
        string JobTitle,
        string Role) : IRequest<PersonAccountDto>;
}
