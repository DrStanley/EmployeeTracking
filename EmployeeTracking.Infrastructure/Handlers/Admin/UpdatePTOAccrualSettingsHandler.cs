using EmployeeTracking.Application.DTOs;
using EmployeeTracking.Application.Interfaces;
using EmployeeTracking.Domain.Common;
using EmployeeTracking.Domain.Entities;
using MediatR;

namespace EmployeeTracking.Infrastructure.Handlers.Admin
{
    public class UpdatePTOAccrualSettingsHandler
        : IRequestHandler<UpdatePTOAccrualSettingsCommand, PTOAccrualSettingsDto>
    {
        private readonly IUnitOfWork _uow;
        public UpdatePTOAccrualSettingsHandler(IUnitOfWork uow) => _uow = uow;

        public async Task<PTOAccrualSettingsDto> Handle(
            UpdatePTOAccrualSettingsCommand request, CancellationToken ct)
        {
            var policy = await _uow.AttendancePolicies
                .GetByIdAsync(request.PolicyId, ct)
                ?? throw new NotFoundException(
                    nameof(AttendancePolicy), request.PolicyId);

            policy.UpdatePTOSettings(
                request.Request.AccrualRatePerPayPeriod,
                request.Request.MaxBalanceHours);

            _uow.AttendancePolicies.Update(policy);
            await _uow.SaveChangesAsync(ct);

            return new PTOAccrualSettingsDto(
                AttendancePolicyId: policy.Id,
                AttendancePolicyName: policy.Name,
                AccrualRatePerPayPeriod: policy.PTOAccrualRatePerPayPeriod,
                MaxBalanceHours: policy.MaxPTOBalanceHours,
                AllowNegativeBalance: false,
                CarryOverLimitHours: (int)policy.MaxPTOBalanceHours);
        }
    }
}
