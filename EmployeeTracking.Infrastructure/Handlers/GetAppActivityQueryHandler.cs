using EmployeeTracking.Application.Activity;
using EmployeeTracking.Application.DTOs;
using EmployeeTracking.Application.Interfaces;
using EmployeeTracking.Application.Queries.Activity;
using EmployeeTracking.Domain.Common;
using EmployeeTracking.Domain.Entities;
using MediatR;

namespace EmployeeTracking.Infrastructure.Handlers
{
    public class GetAppActivityQueryHandler
        : IRequestHandler<GetAppActivityQuery, AppActivityReportDto>
    {
        private readonly IUnitOfWork _uow;

        public GetAppActivityQueryHandler(IUnitOfWork uow) => _uow = uow;

        public async Task<AppActivityReportDto> Handle(
            GetAppActivityQuery request, CancellationToken ct)
        {
            AppActivityCalculator.EnsureRange(request.From, request.To);

            var targetId = request.EmployeeId ?? request.RequesterId;
            if (targetId == Guid.Empty)
                throw new DomainException("Select an employee to view activity.");

            await EnsureCanViewAsync(request, targetId, ct);

            var employee = await _uow.Employees.GetByIdAsync(targetId, ct)
                ?? throw new NotFoundException(nameof(Employee), targetId);

            var sessions = await _uow.AppUsage.GetByEmployeeAndRangeAsync(
                targetId, request.From, request.To, ct);

            var (totalHours, apps) = AppActivityCalculator.Summarize(sessions);

            var sessionDtos = sessions
                .OrderByDescending(s => s.StartedAt)
                .Take(50)
                .Select(s => new AppUsageSessionDto(
                    s.Id,
                    s.AppName,
                    s.StartedAt,
                    s.EndedAt,
                    s.DurationHours,
                    s.Source.ToString()))
                .ToList();

            return new AppActivityReportDto(
                EmployeeId: employee.Id,
                EmployeeName: employee.FullName,
                From: request.From,
                To: request.To,
                TotalHours: totalHours,
                AppCount: apps.Count,
                SessionCount: sessions.Count,
                Apps: apps,
                Sessions: sessionDtos);
        }

        private async Task EnsureCanViewAsync(
            GetAppActivityQuery request, Guid targetId, CancellationToken ct)
        {
            if (targetId == request.RequesterId || request.IsAdmin)
                return;

            if (request.IsManager)
            {
                var reports = await _uow.Employees.GetByManagerAsync(request.RequesterId, ct);
                if (reports.Any(e => e.Id == targetId))
                    return;
            }

            throw new DomainException("You can only view your own activity or your direct reports.");
        }
    }
}
