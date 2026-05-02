using EmployeeTracking.Application.DTOs;
using EmployeeTracking.Domain.Enums;
using MediatR;

namespace EmployeeTracking.Application.Commands.ClockOut
{
    public record ClockOutCommand(
        Guid EmployeeId,
        TimeEntrySource Source,
        double? Latitude,
        double? Longitude,
        string? DeviceId
    ) : IRequest<ClockOutResponse>;
}
