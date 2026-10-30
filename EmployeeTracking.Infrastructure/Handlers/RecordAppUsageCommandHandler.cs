using EmployeeTracking.Application.Commands.Activity;
using EmployeeTracking.Application.DTOs;
using EmployeeTracking.Application.Interfaces;
using EmployeeTracking.Domain.Common;
using EmployeeTracking.Domain.Entities;
using MediatR;

namespace EmployeeTracking.Infrastructure.Handlers
{
    public class RecordAppUsageCommandHandler
        : IRequestHandler<RecordAppUsageCommand, RecordAppUsageResult>
    {
        private readonly IUnitOfWork _uow;

        public RecordAppUsageCommandHandler(IUnitOfWork uow) => _uow = uow;

        public async Task<RecordAppUsageResult> Handle(
            RecordAppUsageCommand request, CancellationToken ct)
        {
            var employee = await _uow.Employees.GetByIdAsync(request.EmployeeId, ct)
                ?? throw new NotFoundException(nameof(Employee), request.EmployeeId);

            if (!employee.IsActive)
                throw new DomainException("Employee profile is inactive.");

            var existing = await _uow.AppUsage.FindDuplicateAsync(
                request.EmployeeId,
                request.AppName.Trim(),
                request.StartedAt,
                request.EndedAt,
                request.Source,
                ct);

            if (existing is not null)
                return new RecordAppUsageResult(existing.Id, existing.DurationHours);

            var session = AppUsageSession.Record(
                request.EmployeeId,
                request.AppName,
                request.StartedAt,
                request.EndedAt,
                request.Source);

            await _uow.AppUsage.AddAsync(session, ct);
            await _uow.SaveChangesAsync(ct);

            return new RecordAppUsageResult(session.Id, session.DurationHours);
        }
    }
}
