using EmployeeTracking.Application.Interfaces;
using EmployeeTracking.Domain.Common;
using EmployeeTracking.Domain.Entities;
using MediatR;

namespace EmployeeTracking.Infrastructure.Handlers.Admin
{
    public class DeleteAttendancePolicyHandler
        : IRequestHandler<DeleteAttendancePolicyCommand, Unit>
    {
        private readonly IUnitOfWork _uow;
        public DeleteAttendancePolicyHandler(IUnitOfWork uow) => _uow = uow;

        public async Task<Unit> Handle(
            DeleteAttendancePolicyCommand request, CancellationToken ct)
        {
            var policy = await _uow.AttendancePolicies
                .GetByIdAsync(request.Id, ct)
                ?? throw new NotFoundException(nameof(AttendancePolicy), request.Id);

            // Prevent deleting a policy that is still assigned to employees
            var assigned = await _uow.Employees.GetAllAsync(ct);
            if (assigned.Any(e => e.AttendancePolicyId == request.Id))
                throw new DomainException(
                    "Cannot delete a policy that is assigned to active employees.");

            _uow.AttendancePolicies.Delete(policy);
            await _uow.SaveChangesAsync(ct);
            return Unit.Value;
        }
    }
}
