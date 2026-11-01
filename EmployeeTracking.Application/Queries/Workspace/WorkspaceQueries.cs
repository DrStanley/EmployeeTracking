using EmployeeTracking.Application.DTOs;
using MediatR;

namespace EmployeeTracking.Application.Queries.Workspace
{
    public record GetWeeklyHoursQuery(Guid EmployeeId) : IRequest<WeeklyHoursDto>;

    public record GetWorkedHoursQuery(
        Guid RequesterId,
        bool IsAdmin,
        bool IsManager,
        Guid? EmployeeId,
        DateTimeOffset From,
        DateTimeOffset To) : IRequest<WorkedHoursDto>;

    public record GetPresenceQuery() : IRequest<IReadOnlyList<PresenceDto>>;

    public record GetEmployeeProfileQuery(Guid EmployeeId) : IRequest<EmployeeProfileDto>;

    public record UpdatePersonalDetailsCommand(
        Guid EmployeeId,
        string FirstName,
        string LastName,
        string? Gender,
        DateOnly? DateOfBirth,
        string? MaritalStatus,
        string? Phone,
        string? AddressLine,
        string? City,
        string? State,
        string? PostalCode,
        string? Country) : IRequest<EmployeeProfileDto>;

    public record GetScheduleQuery(Guid EmployeeId) : IRequest<ScheduleDto>;

    public record GetCalendarMonthQuery(
        Guid RequesterId,
        bool IsAdmin,
        bool IsManager,
        int Year,
        int Month) : IRequest<CalendarMonthDto>;

    public record GetAuditLogsQuery() : IRequest<IReadOnlyList<AuditLogDto>>;
}
