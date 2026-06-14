using EmployeeTracking.Application.DTOs;
using EmployeeTracking.Application.Interfaces;
using EmployeeTracking.Domain.Common;
using EmployeeTracking.Domain.Entities;
using MediatR;

namespace EmployeeTracking.Infrastructure.Handlers.Admin
{
    public class CreateOvertimeRuleHandler
        : IRequestHandler<CreateOvertimeRuleCommand, OvertimeRuleDto>
    {
        private readonly IUnitOfWork _uow;
        public CreateOvertimeRuleHandler(IUnitOfWork uow) => _uow = uow;

        public async Task<OvertimeRuleDto> Handle(
            CreateOvertimeRuleCommand request, CancellationToken ct)
        {
            var policy = await _uow.AttendancePolicies
                .GetByIdAsync(request.Request.AttendancePolicyId, ct)
                ?? throw new NotFoundException(
                    nameof(AttendancePolicy), request.Request.AttendancePolicyId);

            var existing = await _uow.OvertimeRules
                .GetByPolicyAsync(request.Request.AttendancePolicyId, ct);

            if (existing is not null)
                throw new DomainException(
                    "An overtime rule already exists for this policy. " +
                    "Update the existing rule instead.");

            var rule = OvertimeRule.Standard(request.Request.AttendancePolicyId);
            rule.Update(
                request.Request.DailyThresholdHours,
                request.Request.WeeklyThresholdHours,
                request.Request.PremiumMultiplier,
                request.Request.HasDoubleTime,
                request.Request.DoubleTimeDailyThreshold);

            await _uow.OvertimeRules.AddAsync(rule, ct);
            await _uow.SaveChangesAsync(ct);

            return AdminMappers.MapOvertimeRule(rule, policy.Name);
        }
    }
}
