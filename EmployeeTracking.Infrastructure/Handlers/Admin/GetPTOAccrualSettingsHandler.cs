using EmployeeTracking.Application.DTOs;
using EmployeeTracking.Application.Interfaces;
using EmployeeTracking.Domain.Common;
using EmployeeTracking.Domain.Entities;
using MediatR;

namespace EmployeeTracking.Infrastructure.Handlers.Admin
{
    public class GetPTOAccrualSettingsHandler
        : IRequestHandler<GetPTOAccrualSettingsQuery, PTOAccrualSettingsDto>
    {
        private readonly IUnitOfWork _uow;
        public GetPTOAccrualSettingsHandler(IUnitOfWork uow) => _uow = uow;

        public async Task<PTOAccrualSettingsDto> Handle(
            GetPTOAccrualSettingsQuery request, CancellationToken ct)
        {
            var policy = await _uow.AttendancePolicies
                .GetByIdAsync(request.PolicyId, ct)
                ?? throw new NotFoundException(
                    nameof(AttendancePolicy), request.PolicyId);

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
