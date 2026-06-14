using EmployeeTracking.Application.DTOs;
using EmployeeTracking.Application.Interfaces;
using EmployeeTracking.Domain.Common;
using EmployeeTracking.Domain.Entities;
using MediatR;

namespace EmployeeTracking.Infrastructure.Handlers.Admin
{
    public class GetAttendancePolicyHandler
        : IRequestHandler<GetAttendancePolicyQuery, AttendancePolicyDto>
    {
        private readonly IUnitOfWork _uow;
        public GetAttendancePolicyHandler(IUnitOfWork uow) => _uow = uow;

        public async Task<AttendancePolicyDto> Handle(
            GetAttendancePolicyQuery request, CancellationToken ct)
        {
            var policy = await _uow.AttendancePolicies.GetByIdAsync(request.Id, ct)
                ?? throw new NotFoundException(nameof(AttendancePolicy), request.Id);
            return AdminMappers.MapPolicy(policy);
        }
    }
}
