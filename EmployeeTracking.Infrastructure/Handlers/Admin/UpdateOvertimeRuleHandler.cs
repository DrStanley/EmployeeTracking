using EmployeeTracking.Application.DTOs;
using EmployeeTracking.Application.Interfaces;
using EmployeeTracking.Domain.Common;
using EmployeeTracking.Domain.Entities;
using MediatR;

namespace EmployeeTracking.Infrastructure.Handlers.Admin
{
    public class UpdateOvertimeRuleHandler
        : IRequestHandler<UpdateOvertimeRuleCommand, OvertimeRuleDto>
    {
        private readonly IUnitOfWork _uow;
        public UpdateOvertimeRuleHandler(IUnitOfWork uow) => _uow = uow;

        public async Task<OvertimeRuleDto> Handle(
            UpdateOvertimeRuleCommand request, CancellationToken ct)
        {
            var rule = await _uow.OvertimeRules.GetByIdAsync(request.Id, ct)
                ?? throw new NotFoundException(nameof(OvertimeRule), request.Id);

            rule.Update(
                request.Request.DailyThresholdHours,
                request.Request.WeeklyThresholdHours,
                request.Request.PremiumMultiplier,
                request.Request.HasDoubleTime,
                request.Request.DoubleTimeDailyThreshold);

            _uow.OvertimeRules.Update(rule);
            await _uow.SaveChangesAsync(ct);

            return AdminMappers.MapOvertimeRule(rule, rule.AttendancePolicy?.Name ?? string.Empty);
        }
    }
}
