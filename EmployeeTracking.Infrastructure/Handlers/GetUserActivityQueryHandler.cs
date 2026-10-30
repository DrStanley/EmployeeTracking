using EmployeeTracking.Application.Activity;
using EmployeeTracking.Application.DTOs;
using EmployeeTracking.Application.Interfaces;
using EmployeeTracking.Application.Queries.Activity;
using EmployeeTracking.Domain.Entities;
using MediatR;

namespace EmployeeTracking.Infrastructure.Handlers
{
    public class GetUserActivityQueryHandler
        : IRequestHandler<GetUserActivityQuery, IReadOnlyList<UserActivitySummaryDto>>
    {
        private readonly IUnitOfWork _uow;

        public GetUserActivityQueryHandler(IUnitOfWork uow) => _uow = uow;

        public async Task<IReadOnlyList<UserActivitySummaryDto>> Handle(
            GetUserActivityQuery request, CancellationToken ct)
        {
            AppActivityCalculator.EnsureRange(request.From, request.To);

            IReadOnlyList<Employee> employees;
            if (request.IsAdmin)
                employees = await _uow.Employees.GetAllAsync(ct);
            else if (request.IsManager)
                employees = await _uow.Employees.GetByManagerAsync(request.RequesterId, ct);
            else
            {
                var self = await _uow.Employees.GetByIdAsync(request.RequesterId, ct);
                employees = self is null ? Array.Empty<Employee>() : new[] { self };
            }

            var ids = employees.Select(e => e.Id).ToList();
            var sessions = await _uow.AppUsage.GetByEmployeesAndRangeAsync(
                ids, request.From, request.To, ct);

            var byEmployee = sessions
                .GroupBy(s => s.EmployeeId)
                .ToDictionary(g => g.Key, g => (IReadOnlyList<AppUsageSession>)g.ToList());

            return employees
                .Select(employee =>
                {
                    byEmployee.TryGetValue(employee.Id, out var mine);
                    var (hours, apps) = AppActivityCalculator.Summarize(mine ?? Array.Empty<AppUsageSession>());
                    var top = apps.FirstOrDefault();
                    return new UserActivitySummaryDto(
                        employee.Id,
                        employee.FullName,
                        employee.JobTitle,
                        hours,
                        apps.Count,
                        top?.AppName,
                        top?.Hours ?? 0);
                })
                .OrderByDescending(u => u.TotalHours)
                .ThenBy(u => u.EmployeeName, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }
    }
}
