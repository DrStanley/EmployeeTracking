using EmployeeTracking.Application.DTOs;
using EmployeeTracking.Domain.Enums;
using MediatR;

namespace EmployeeTracking.Application.Commands.Activity
{
    public record RecordAppUsageCommand(
        Guid EmployeeId,
        string AppName,
        DateTimeOffset StartedAt,
        DateTimeOffset EndedAt,
        AppUsageSource Source
    ) : IRequest<RecordAppUsageResult>;
}
