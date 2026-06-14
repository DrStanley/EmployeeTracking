using EmployeeTracking.Application.DTOs;
using EmployeeTracking.Application.Interfaces;
using MediatR;

namespace EmployeeTracking.Infrastructure.Handlers.Admin
{
    public class GetAllAttendancePoliciesHandler
        : IRequestHandler<GetAllAttendancePoliciesQuery, IReadOnlyList<AttendancePolicyDto>>
    {
        private readonly IUnitOfWork _uow;
        public GetAllAttendancePoliciesHandler(IUnitOfWork uow) => _uow = uow;

        public async Task<IReadOnlyList<AttendancePolicyDto>> Handle(
            GetAllAttendancePoliciesQuery request, CancellationToken ct)
            => (await _uow.AttendancePolicies.GetAllAsync(ct))
                .Select(AdminMappers.MapPolicy).ToList();
    }
}
