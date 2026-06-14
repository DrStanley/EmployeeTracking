using EmployeeTracking.Application.DTOs;
using EmployeeTracking.Application.Interfaces;
using EmployeeTracking.Domain.Entities;
using MediatR;

namespace EmployeeTracking.Infrastructure.Handlers.Admin
{
    //Handlers

    public class CreateAttendancePolicyHandler
        : IRequestHandler<CreateAttendancePolicyCommand, AttendancePolicyDto>
    {
        private readonly IUnitOfWork _uow;
        public CreateAttendancePolicyHandler(IUnitOfWork uow) => _uow = uow;

        public async Task<AttendancePolicyDto> Handle(
            CreateAttendancePolicyCommand request, CancellationToken ct)
        {
            var req = request.Request;
            var policy = AttendancePolicy.CreateDefault(req.Name);

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

            await _uow.AttendancePolicies.AddAsync(policy, ct);
            await _uow.SaveChangesAsync(ct);

            return AdminMappers.MapPolicy(policy);
        }
    }
}
