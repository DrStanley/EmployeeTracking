using EmployeeTracking.Application.DTOs;
using EmployeeTracking.Application.Interfaces;
using EmployeeTracking.Application.Queries.PTO;
using MediatR;

namespace EmployeeTracking.Infrastructure.Handlers
{
    public class GetPendingPTOForManagerQueryHandler
        : IRequestHandler<GetPendingPTOForManagerQuery, IReadOnlyList<PTORequestDto>>
    {
        private readonly IUnitOfWork _uow;

        public GetPendingPTOForManagerQueryHandler(IUnitOfWork uow) => _uow = uow;

        public async Task<IReadOnlyList<PTORequestDto>> Handle(
            GetPendingPTOForManagerQuery request, CancellationToken ct)
        {
            var pending = await _uow.PTORequests
                .GetPendingByManagerAsync(request.ManagerId, ct);

            var result = new List<PTORequestDto>();

            foreach (var pto in pending)
            {
                var employee = await _uow.Employees.GetByIdAsync(pto.EmployeeId, ct);
                result.Add(PTOMapHelper.MapToDto(pto, employee!, null));
            }

            return result;
        }
    }
}
