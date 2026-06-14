using EmployeeTracking.Application.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmployeeTracking.Infrastructure.Handlers.Admin
{
    public record CreateAttendancePolicyCommand(
     CreateAttendancePolicyRequest Request) : IRequest<AttendancePolicyDto>;

    public record UpdateAttendancePolicyCommand(
        Guid Id, UpdateAttendancePolicyRequest Request) : IRequest<AttendancePolicyDto>;

    public record DeleteAttendancePolicyCommand(Guid Id) : IRequest<Unit>;

    public record CreateOvertimeRuleCommand(
        CreateOvertimeRuleRequest Request) : IRequest<OvertimeRuleDto>;

    public record UpdateOvertimeRuleCommand(
        Guid Id, UpdateOvertimeRuleRequest Request) : IRequest<OvertimeRuleDto>;

    public record DeleteOvertimeRuleCommand(Guid Id) : IRequest<Unit>;

    public record CreateShiftCommand(
        CreateShiftRequest Request) : IRequest<ShiftDto>;

    public record UpdateShiftCommand(
        Guid Id, UpdateShiftRequest Request) : IRequest<ShiftDto>;

    public record DeleteShiftCommand(Guid Id) : IRequest<Unit>;

    public record CreateHolidayCommand(
        CreateHolidayRequest Request) : IRequest<HolidayDto>;

    public record UpdateHolidayCommand(
        Guid Id, UpdateHolidayRequest Request) : IRequest<HolidayDto>;

    public record DeleteHolidayCommand(Guid Id) : IRequest<Unit>;

    public record UpdatePTOAccrualSettingsCommand(
        Guid PolicyId,
        UpdatePTOAccrualSettingsRequest Request) : IRequest<PTOAccrualSettingsDto>;

    public record CreatePayPeriodCommand(
        CreatePayPeriodRequest Request) : IRequest<PayPeriodDto>;

    public record LockPayPeriodCommand(Guid Id) : IRequest<PayPeriodDto>;

    //Queries
    public record GetAllAttendancePoliciesQuery() : IRequest<IReadOnlyList<AttendancePolicyDto>>;
    public record GetAttendancePolicyQuery(Guid Id) : IRequest<AttendancePolicyDto>;
    public record GetAllOvertimeRulesQuery() : IRequest<IReadOnlyList<OvertimeRuleDto>>;
    public record GetAllShiftsQuery() : IRequest<IReadOnlyList<ShiftDto>>;
    public record GetHolidaysQuery(int Year) : IRequest<IReadOnlyList<HolidayDto>>;
    public record GetAllPayPeriodsQuery() : IRequest<IReadOnlyList<PayPeriodDto>>;
    public record GetPTOAccrualSettingsQuery(Guid PolicyId) : IRequest<PTOAccrualSettingsDto>;
}
