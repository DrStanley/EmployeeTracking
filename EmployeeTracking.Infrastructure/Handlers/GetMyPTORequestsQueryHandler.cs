using EmployeeTracking.Application.DTOs;
using EmployeeTracking.Application.Interfaces;
using EmployeeTracking.Application.Queries.PTO;
using EmployeeTracking.Domain.Common;
using EmployeeTracking.Domain.Entities;
using MediatR;

namespace EmployeeTracking.Infrastructure.Handlers
{
    public class GetMyPTORequestsQueryHandler
    : IRequestHandler<GetMyPTORequestsQuery, IReadOnlyList<PTORequestDto>>
    {
        private readonly IUnitOfWork _uow;

        public GetMyPTORequestsQueryHandler(IUnitOfWork uow) => _uow = uow;

        public async Task<IReadOnlyList<PTORequestDto>> Handle(
            GetMyPTORequestsQuery request, CancellationToken ct)
        {
            var employee = await _uow.Employees.GetByIdAsync(request.EmployeeId, ct)
                ?? throw new NotFoundException(nameof(Employee), request.EmployeeId);

            var requests = await _uow.PTORequests
                .GetByEmployeeAsync(request.EmployeeId, ct);

            return requests
                .Select(p => PTOMapHelper.MapToDto(p, employee, null))
                .ToList();
        }
    }
}
