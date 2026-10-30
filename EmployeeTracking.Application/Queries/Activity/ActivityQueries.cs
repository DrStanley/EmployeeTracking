using EmployeeTracking.Application.DTOs;
using MediatR;

namespace EmployeeTracking.Application.Queries.Activity
{
    public record GetAppActivityQuery(
        Guid RequesterId,
        bool IsAdmin,
        bool IsManager,
        Guid? EmployeeId,
        DateTimeOffset From,
        DateTimeOffset To
    ) : IRequest<AppActivityReportDto>;

    public record GetUserActivityQuery(
        Guid RequesterId,
        bool IsAdmin,
        bool IsManager,
        DateTimeOffset From,
        DateTimeOffset To
    ) : IRequest<IReadOnlyList<UserActivitySummaryDto>>;
}
