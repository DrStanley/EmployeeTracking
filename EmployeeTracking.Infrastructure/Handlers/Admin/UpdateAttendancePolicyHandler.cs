using EmployeeTracking.Application.DTOs;
using EmployeeTracking.Application.Interfaces;
using EmployeeTracking.Domain.Common;
using EmployeeTracking.Domain.Entities;
using MediatR;

namespace EmployeeTracking.Infrastructure.Handlers.Admin
{
    public class UpdateAttendancePolicyHandler
        : IRequestHandler<UpdateAttendancePolicyCommand, AttendancePolicyDto>
    {
        private readonly IUnitOfWork _uow;
        public UpdateAttendancePolicyHandler(IUnitOfWork uow) => _uow = uow;

        public async Task<AttendancePolicyDto> Handle(
            UpdateAttendancePolicyCommand request, CancellationToken ct)
        {
            var policy = await _uow.AttendancePolicies
                .GetByIdAsync(request.Id, ct)
                ?? throw new NotFoundException(nameof(AttendancePolicy), request.Id);

            var req = request.Request;
            policy.Update(
                req.Name,
                req.DailyOvertimeThresholdHours,
                req.WeeklyOvertimeThresholdHours,
                req.OvertimeMultiplier,
                req.HasPaidBreaks,
                req.PaidBreakMinutes,
                req.UnpaidBreakMinutes);

            policy.UpdatePTOSettings(
                req.PTOAccrualRatePerPayPeriod,
                req.MaxPTOBalanceHours);

            _uow.AttendancePolicies.Update(policy);
            await _uow.SaveChangesAsync(ct);

            return AdminMappers.MapPolicy(policy);
        }
    }
}
