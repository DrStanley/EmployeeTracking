using EmployeeTracking.Application.DTOs;
using EmployeeTracking.Application.Interfaces;
using MediatR;

namespace EmployeeTracking.Infrastructure.Handlers.Admin
{
    public class GetAllShiftsHandler
        : IRequestHandler<GetAllShiftsQuery, IReadOnlyList<ShiftDto>>
    {
        private readonly IUnitOfWork _uow;
        public GetAllShiftsHandler(IUnitOfWork uow) => _uow = uow;

        public async Task<IReadOnlyList<ShiftDto>> Handle(
            GetAllShiftsQuery request, CancellationToken ct)
            => (await _uow.Shifts.GetAllAsync(ct))
                .Select(AdminMappers.MapShift).ToList();
    }
}
