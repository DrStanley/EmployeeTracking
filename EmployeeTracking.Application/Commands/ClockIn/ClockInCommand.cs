using EmployeeTracking.Application.DTOs;
using EmployeeTracking.Domain.Enums;
using MediatR;

namespace EmployeeTracking.Application.Commands.ClockIn
{

    public record ClockInCommand(
        Guid EmployeeId,
        TimeEntrySource Source,
        double? Latitude,
        double? Longitude,
        string? DeviceId
    ) : IRequest<ClockInResponse>;
}
