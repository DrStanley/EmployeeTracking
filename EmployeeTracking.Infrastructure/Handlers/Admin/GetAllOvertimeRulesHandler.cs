using EmployeeTracking.Application.DTOs;
using EmployeeTracking.Application.Interfaces;
using MediatR;

namespace EmployeeTracking.Infrastructure.Handlers.Admin
{
    public class GetAllOvertimeRulesHandler
        : IRequestHandler<GetAllOvertimeRulesQuery, IReadOnlyList<OvertimeRuleDto>>
    {
        private readonly IUnitOfWork _uow;
        public GetAllOvertimeRulesHandler(IUnitOfWork uow) => _uow = uow;

        public async Task<IReadOnlyList<OvertimeRuleDto>> Handle(
            GetAllOvertimeRulesQuery request, CancellationToken ct)
            => (await _uow.OvertimeRules.GetAllAsync(ct))
                .Select(r => AdminMappers.MapOvertimeRule(r, r.AttendancePolicy?.Name ?? string.Empty))
                .ToList();
    }
}
